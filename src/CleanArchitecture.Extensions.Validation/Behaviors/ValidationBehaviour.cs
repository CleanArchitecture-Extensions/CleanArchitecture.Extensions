using System.Reflection;
using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Options;
using CleanArchitecture.Extensions.Core.Results;
using CleanArchitecture.Extensions.Validation.Exceptions;
using CleanArchitecture.Extensions.Validation.Models;
using CleanArchitecture.Extensions.Validation.Notifications;
using CleanArchitecture.Extensions.Validation.Options;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Options;
using ValidationException = CleanArchitecture.Extensions.Validation.Exceptions.ValidationException;

namespace CleanArchitecture.Extensions.Validation.Behaviors;

/// <summary>
/// MediatR pipeline behaviour that executes FluentValidation validators and surfaces failures using configured strategy.
/// </summary>
/// <typeparam name="TRequest">Request type.</typeparam>
/// <typeparam name="TResponse">Response type.</typeparam>
public sealed class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IReadOnlyCollection<IValidator<TRequest>> _validators;
    private readonly ValidationOptions _options;
    private readonly IValidationNotificationPublisher? _notificationPublisher;
    private readonly ILogContext? _logContext;
    private readonly CoreExtensionsOptions? _coreOptions;
    private readonly IAppLogger<TRequest>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBehaviour{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="validators">Validators to execute.</param>
    /// <param name="options">Validation behavior options.</param>
    /// <param name="notificationPublisher">Optional publisher for validation notifications.</param>
    /// <param name="logContext">Optional log context providing correlation identifiers.</param>
    /// <param name="coreOptions">Optional shared core options for default trace identifiers.</param>
    /// <param name="logger">Optional logger for emitting validation summaries.</param>
    public ValidationBehaviour(
        IEnumerable<IValidator<TRequest>> validators,
        IOptions<ValidationOptions>? options = null,
        IValidationNotificationPublisher? notificationPublisher = null,
        ILogContext? logContext = null,
        IOptions<CoreExtensionsOptions>? coreOptions = null,
        IAppLogger<TRequest>? logger = null)
    {
        _validators = (validators ?? throw new ArgumentNullException(nameof(validators))).ToArray();
        _options = options?.Value ?? ValidationOptions.Default;
        _notificationPublisher = notificationPublisher;
        _logContext = logContext;
        _coreOptions = coreOptions?.Value;
        _logger = logger;
    }

    /// <summary>
    /// Executes validators and applies the configured strategy when failures occur.
    /// </summary>
    /// <param name="request">Incoming request.</param>
    /// <param name="next">Delegate to invoke the next handler.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Pipeline response or short-circuited result.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Count == 0)
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken))).ConfigureAwait(false);

        var failures = validationResults
            .Where(result => result.Errors is not null && result.Errors.Count != 0)
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }

        var traceId = ResolveTraceId();
        var limitedFailures = failures.Take(_options.MaxFailures).ToList();
        var validationErrors = limitedFailures
            .Select(failure => ValidationError.FromFailure(failure, _options))
            .ToList();

        LogValidationFailures(limitedFailures, failures.Count, traceId);

        var supportsResultResponse = IsResultResponse(typeof(TResponse), out var valueType);
        MethodInfo? templateFailureMethod = null;
        if (!supportsResultResponse)
        {
            supportsResultResponse = TryLocateTemplateResult(typeof(TResponse), out templateFailureMethod);
        }

        if (_options.Strategy == ValidationStrategy.Notify)
        {
            if (_notificationPublisher is not null)
            {
                await _notificationPublisher.PublishAsync(validationErrors, cancellationToken).ConfigureAwait(false);
            }

            if (_options.NotifyBehavior == ValidationNotifyBehavior.Throw || !supportsResultResponse)
            {
                throw new ValidationException(limitedFailures);
            }

            return CreateResultResponse(validationErrors, traceId, valueType, templateFailureMethod);
        }

        if (_options.Strategy == ValidationStrategy.ReturnResult)
        {
            if (!supportsResultResponse)
            {
                throw new ValidationException(limitedFailures);
            }

            return CreateResultResponse(validationErrors, traceId, valueType, templateFailureMethod);
        }

        throw new ValidationException(limitedFailures);
    }

    private TResponse CreateResultResponse(
        IReadOnlyCollection<ValidationError> errors,
        string? traceId,
        Type? valueType,
        MethodInfo? templateFailureMethod)
    {
        var mappedErrors = errors.Select(error => error.ToCoreError(traceId)).ToList();

        if (templateFailureMethod is not null)
        {
            var messages = errors.Select(error => error.Message);
            var template = templateFailureMethod.Invoke(null, new object?[] { messages });
            return (TResponse)template!;
        }

        if (valueType is null)
        {
            return (TResponse)(object)Result.Failure(mappedErrors, traceId);
        }

        var genericFailure = CreateGenericFailure(valueType, mappedErrors, traceId);
        return (TResponse)genericFailure;

        static object CreateGenericFailure(Type valueType, IReadOnlyCollection<Error> mappedErrors, string? trace)
        {
            var failureMethod = typeof(Result)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(method =>
                    method.IsGenericMethod &&
                    method.Name == nameof(Result.Failure) &&
                    method.GetParameters().Length == 2 &&
                    method.GetParameters()[0].ParameterType == typeof(IEnumerable<Error>));

            var closed = failureMethod.MakeGenericMethod(valueType);
            return closed.Invoke(null, new object?[] { mappedErrors, trace })!;
        }
    }

    private string? ResolveTraceId()
    {
        if (!string.IsNullOrWhiteSpace(_options.TraceId))
        {
            return _options.TraceId;
        }

        if (!string.IsNullOrWhiteSpace(_logContext?.CorrelationId))
        {
            return _logContext!.CorrelationId;
        }

        return _coreOptions?.TraceId;
    }

    private void LogValidationFailures(IReadOnlyCollection<ValidationFailure> failures, int totalFailureCount, string? traceId)
    {
        if (_logger is null || !_options.LogValidationFailures)
        {
            return;
        }

        var logLevel = ResolveLogLevel(failures);
        if (logLevel == LogLevel.None)
        {
            return;
        }

        var properties = new Dictionary<string, object?>
        {
            ["RequestType"] = typeof(TRequest).FullName ?? typeof(TRequest).Name,
            ["CorrelationId"] = _logContext?.CorrelationId,
            ["TraceId"] = traceId,
            ["FailureCount"] = failures.Count,
            ["TotalFailureCount"] = totalFailureCount,
            ["Truncated"] = totalFailureCount > failures.Count,
            ["Errors"] = failures.Select(f => new
            {
                f.PropertyName,
                f.ErrorCode,
                f.ErrorMessage,
                Severity = f.Severity.ToString()
            }).ToList()
        };

        _logger.Log(logLevel, $"Validation failed for {typeof(TRequest).Name}", null, properties);
    }

    private LogLevel ResolveLogLevel(IEnumerable<ValidationFailure> failures)
    {
        LogLevel? highestLevel = null;
        var severityMap = _options.SeverityLogLevels ?? ValidationOptions.Default.SeverityLogLevels;

        foreach (var failure in failures)
        {
            var level = _options.DefaultLogLevel;

            if (severityMap.TryGetValue(failure.Severity, out var mappedLevel))
            {
                level = mappedLevel;
            }

            if (highestLevel is null || level > highestLevel)
            {
                highestLevel = level;
            }
        }

        return highestLevel ?? _options.DefaultLogLevel;
    }

    private static bool IsResultResponse(Type responseType, out Type? valueType)
    {
        if (responseType == typeof(Result))
        {
            valueType = null;
            return true;
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            valueType = responseType.GetGenericArguments()[0];
            return true;
        }

        valueType = null;
        return false;
    }

    private static bool TryLocateTemplateResult(Type responseType, out MethodInfo? failureMethod)
    {
        failureMethod = null;
        if (!string.Equals(responseType.Name, "Result", StringComparison.Ordinal))
        {
            return false;
        }

        failureMethod = responseType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(method =>
                method.Name == "Failure" &&
                method.GetParameters().Length == 1 &&
                typeof(IEnumerable<string>).IsAssignableFrom(method.GetParameters()[0].ParameterType) &&
                method.ReturnType == responseType);

        return failureMethod is not null;
    }
}
