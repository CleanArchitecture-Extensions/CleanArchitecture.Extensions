using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Options;
using CleanArchitecture.Extensions.Core.Results;
using CleanArchitecture.Extensions.Exceptions.BaseTypes;
using CleanArchitecture.Extensions.Exceptions.Catalog;
using CleanArchitecture.Extensions.Exceptions.Options;
using CleanArchitecture.Extensions.Exceptions.Redaction;
using MediatR;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Exceptions.Behaviors;

/// <summary>
/// MediatR pipeline behaviour that wraps unhandled exceptions, logs them, and optionally converts them to <see cref="Result"/>/<see cref="Result{T}"/>.
/// </summary>
/// <typeparam name="TRequest">Request type.</typeparam>
/// <typeparam name="TResponse">Response type.</typeparam>
public sealed class ExceptionWrappingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IExceptionCatalog _catalog;
    private readonly ExceptionHandlingOptions _options;
    private readonly ExceptionRedactor _redactor;
    private readonly IAppLogger<TRequest>? _logger;
    private readonly ILogContext? _logContext;
    private readonly CoreExtensionsOptions? _coreOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionWrappingBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="catalog">Exception catalog used for mapping.</param>
    /// <param name="options">Exception handling options.</param>
    /// <param name="redactor">Redactor used to scrub sensitive values from messages/metadata.</param>
    /// <param name="logger">Optional logger for emitting structured entries.</param>
    /// <param name="logContext">Optional log context used to capture correlation identifiers.</param>
    /// <param name="coreOptions">Optional core options for default trace identifiers.</param>
    public ExceptionWrappingBehavior(
        IExceptionCatalog catalog,
        IOptions<ExceptionHandlingOptions>? options = null,
        ExceptionRedactor? redactor = null,
        IAppLogger<TRequest>? logger = null,
        ILogContext? logContext = null,
        IOptions<CoreExtensionsOptions>? coreOptions = null)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _options = options?.Value ?? ExceptionHandlingOptions.Default;
        _redactor = redactor ?? new ExceptionRedactor();
        _logger = logger;
        _logContext = logContext;
        _coreOptions = coreOptions?.Value;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var normalized = NormalizeException(ex);

            if (ShouldRethrow(normalized))
            {
                throw;
            }

            return HandleException(normalized);
        }
    }

    private TResponse HandleException(Exception exception)
    {
        var descriptor = _catalog.Resolve(exception);
        var traceId = ResolveTraceId();
        var error = descriptor.ToError(exception, traceId, _options.IncludeExceptionDetails, _options.RedactSensitiveData, _redactor);

        LogException(exception, descriptor, error);

        var templateFailureMethod = default(MethodInfo);
        var hasResultResponse = IsResultResponse(out var valueType);
        if (!hasResultResponse)
        {
            hasResultResponse = TryLocateTemplateResult(out templateFailureMethod);
        }

        if (_options.ConvertToResult && hasResultResponse)
        {
            return CreateResultResponse(error, valueType, traceId, templateFailureMethod);
        }

        ExceptionDispatchInfo.Capture(exception).Throw();
        return default!;
    }

    private bool ShouldRethrow(Exception exception)
    {
        if (_options.RethrowCancellationExceptions && exception is OperationCanceledException)
        {
            return true;
        }

        return _options.RethrowExceptionTypes.Any(type => type.IsAssignableFrom(exception.GetType()));
    }

    private bool IsResultResponse(out Type? valueType)
    {
        var responseType = typeof(TResponse);

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

    private TResponse CreateResultResponse(Error error, Type? valueType, string? traceId, MethodInfo? templateFailureMethod)
    {
        if (templateFailureMethod is not null)
        {
            var formatted = string.IsNullOrWhiteSpace(error.Message) ? error.Code : $"{error.Code}: {error.Message}";
            var template = templateFailureMethod.Invoke(null, new object?[] { new[] { formatted } });
            return (TResponse)template!;
        }

        if (valueType is null)
        {
            return (TResponse)(object)Result.Failure(error, traceId);
        }

        var failure = ResultFailureFactory.CreateGenericFailure(valueType, new[] { error }, traceId);
        return (TResponse)failure;
    }

    private static bool TryLocateTemplateResult(out MethodInfo? failureMethod)
    {
        failureMethod = null;
        var responseType = typeof(TResponse);
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

    private void LogException(Exception exception, ExceptionDescriptor descriptor, Error error)
    {
        if (!_options.LogExceptions || _logger is null)
        {
            return;
        }

        var level = MapLogLevel(descriptor.Severity);
        if (level == LogLevel.None)
        {
            return;
        }

        var includeDetails = _options.IncludeExceptionDetails;
        var logMessage = includeDetails ? exception.Message : descriptor.Message;
        if (_options.RedactSensitiveData)
        {
            logMessage = _redactor.Redact(logMessage);
        }

        var properties = new Dictionary<string, object?>
        {
            ["RequestType"] = typeof(TRequest).FullName ?? typeof(TRequest).Name,
            ["CorrelationId"] = _logContext?.CorrelationId,
            ["TraceId"] = error.TraceId,
            ["ErrorCode"] = error.Code,
            ["ExceptionType"] = exception.GetType().FullName ?? exception.GetType().Name,
            ["IsTransient"] = descriptor.IsTransient,
            ["Severity"] = descriptor.Severity.ToString()
        };

        if (descriptor.StatusCode.HasValue)
        {
            properties["StatusCode"] = (int)descriptor.StatusCode.Value;
        }

        properties["ExceptionMessage"] = logMessage;

        // Avoid logging full exception when details are disabled or must be redacted.
        var exceptionToLog = includeDetails && !_options.RedactSensitiveData ? exception : null;
        _logger.Log(level, $"Unhandled exception for {typeof(TRequest).Name}: {logMessage}", exceptionToLog, properties);
    }

    private static LogLevel MapLogLevel(ExceptionSeverity severity) => severity switch
    {
        ExceptionSeverity.Info => LogLevel.Information,
        ExceptionSeverity.Warning => LogLevel.Warning,
        ExceptionSeverity.Critical => LogLevel.Critical,
        _ => LogLevel.Error
    };

    private static Exception NormalizeException(Exception exception)
    {
        if (exception is AggregateException aggregate && aggregate.InnerExceptions.Count == 1)
        {
            return aggregate.InnerExceptions[0];
        }

        return exception;
    }
}
