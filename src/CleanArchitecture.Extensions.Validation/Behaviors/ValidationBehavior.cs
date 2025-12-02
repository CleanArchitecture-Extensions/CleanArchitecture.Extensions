using System.Reflection;
using CleanArchitecture.Extensions.Core.Results;
using CleanArchitecture.Extensions.Validation.Exceptions;
using CleanArchitecture.Extensions.Validation.Models;
using CleanArchitecture.Extensions.Validation.Notifications;
using CleanArchitecture.Extensions.Validation.Options;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using ValidationException = CleanArchitecture.Extensions.Validation.Exceptions.ValidationException;

namespace CleanArchitecture.Extensions.Validation.Behaviors;

/// <summary>
/// MediatR pipeline behavior that executes FluentValidation validators and surfaces failures using configured strategy.
/// </summary>
/// <typeparam name="TRequest">Request type.</typeparam>
/// <typeparam name="TResponse">Response type.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IReadOnlyCollection<IValidator<TRequest>> _validators;
    private readonly ValidationOptions _options;
    private readonly IValidationNotificationPublisher? _notificationPublisher;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="validators">Validators to execute.</param>
    /// <param name="options">Validation behavior options.</param>
    /// <param name="notificationPublisher">Optional publisher for validation notifications.</param>
    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ValidationOptions? options = null,
        IValidationNotificationPublisher? notificationPublisher = null)
    {
        _validators = (validators ?? throw new ArgumentNullException(nameof(validators))).ToArray();
        _options = options ?? ValidationOptions.Default;
        _notificationPublisher = notificationPublisher;
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

        var limitedFailures = failures.Take(_options.MaxFailures).ToList();
        var validationErrors = limitedFailures
            .Select(failure => ValidationError.FromFailure(failure, _options))
            .ToList();

        if (_options.Strategy == ValidationStrategy.Notify)
        {
            if (_notificationPublisher is not null)
            {
                await _notificationPublisher.PublishAsync(validationErrors, cancellationToken).ConfigureAwait(false);
            }

            if (_options.NotifyBehavior == ValidationNotifyBehavior.Throw)
            {
                throw new ValidationException(limitedFailures);
            }

            return CreateResultOrThrow(validationErrors, limitedFailures);
        }

        if (_options.Strategy == ValidationStrategy.ReturnResult)
        {
            return CreateResultOrThrow(validationErrors, limitedFailures);
        }

        throw new ValidationException(limitedFailures);
    }

    private TResponse CreateResultOrThrow(IReadOnlyCollection<ValidationError> errors, List<ValidationFailure> failures)
    {
        var responseType = typeof(TResponse);
        var traceId = _options.TraceId;
        var mappedErrors = errors.Select(error => error.ToCoreError(traceId)).ToList();

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(mappedErrors, traceId);
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = responseType.GetGenericArguments()[0];
            var genericFailure = CreateGenericFailure(valueType, mappedErrors, traceId);
            return (TResponse)genericFailure;
        }

        throw new InvalidOperationException(
            "ValidationBehavior is configured to return a Result, but TResponse is not a Result or Result<T>. " +
            "Use ValidationStrategy.Throw for non-Result handlers or update handlers to return Result.");
    }

    private static object CreateGenericFailure(Type valueType, IReadOnlyCollection<Error> errors, string? traceId)
    {
        var failureMethod = typeof(Result)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(method =>
                method.IsGenericMethod &&
                method.Name == nameof(Result.Failure) &&
                method.GetParameters().Length == 2 &&
                method.GetParameters()[0].ParameterType == typeof(IEnumerable<Error>));

        var closed = failureMethod.MakeGenericMethod(valueType);
        return closed.Invoke(null, new object?[] { errors, traceId })!;
    }
}
