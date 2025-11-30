namespace CleanArchitecture.Extensions.Core.Results;

/// <summary>
/// Represents the outcome of an operation that produces a value when successful.
/// </summary>
/// <typeparam name="T">Type of the value returned on success.</typeparam>
public class Result<T> : Result
{
    private readonly T? _value;

    private Result(T? value, bool isSuccess, string? traceId, IReadOnlyList<Error> errors)
        : base(isSuccess, traceId, errors)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the value produced by the operation when successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing on a failed result.</exception>
    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Cannot access value on a failed result.");

    /// <summary>
    /// Gets the value if available; otherwise returns the default value for <typeparamref name="T"/>.
    /// </summary>
    public T? ValueOrDefault => _value;

    /// <summary>
    /// Creates a successful result containing the provided value.
    /// </summary>
    /// <param name="value">Value to carry.</param>
    /// <param name="traceId">Optional trace identifier to attach.</param>
    /// <returns>A successful <see cref="Result{T}"/>.</returns>
    public static Result<T> Success(T value, string? traceId = null) => new(value, true, traceId, Array.Empty<Error>());

    /// <summary>
    /// Creates a failed result with a single error.
    /// </summary>
    /// <param name="error">Error describing the failure.</param>
    /// <param name="traceId">Optional trace identifier to attach or override.</param>
    /// <returns>A failed <see cref="Result{T}"/>.</returns>
    public new static Result<T> Failure(Error error, string? traceId = null)
    {
        var resolvedTraceId = traceId ?? error.TraceId;
        return new(default, false, resolvedTraceId, new[] { error.WithTraceId(resolvedTraceId) });
    }

    /// <summary>
    /// Creates a failed result with multiple errors.
    /// </summary>
    /// <param name="errors">Errors describing the failure.</param>
    /// <param name="traceId">Optional trace identifier to attach or override.</param>
    /// <returns>A failed <see cref="Result{T}"/>.</returns>
    public new static Result<T> Failure(IEnumerable<Error> errors, string? traceId = null)
    {
        var errorList = errors.ToList();
        var resolvedTraceId = traceId ?? errorList.FirstOrDefault().TraceId;
        var normalizedErrors = errorList.Select(error => error.WithTraceId(resolvedTraceId)).ToList();
        return new(default, false, resolvedTraceId, normalizedErrors);
    }

    /// <summary>
    /// Maps the value of a successful result to a new result, propagating failures unchanged.
    /// </summary>
    /// <typeparam name="TResult">Target value type.</typeparam>
    /// <param name="mapper">Function to transform the value.</param>
    /// <param name="traceId">Optional trace identifier to attach or override.</param>
    /// <returns>A result containing the mapped value or the original errors.</returns>
    public Result<TResult> Map<TResult>(Func<T, TResult> mapper, string? traceId = null)
    {
        if (mapper is null)
        {
            throw new ArgumentNullException(nameof(mapper));
        }
        return IsFailure
            ? Result.Failure<TResult>(Errors, traceId ?? TraceId)
            : Result.Success(mapper(Value), traceId ?? TraceId);
    }

    /// <summary>
    /// Binds a successful result to another result-producing function, enabling monadic composition.
    /// </summary>
    /// <typeparam name="TResult">Target value type.</typeparam>
    /// <param name="binder">Function that consumes the value and returns a new result.</param>
    /// <param name="traceId">Optional trace identifier to attach or override.</param>
    /// <returns>The result returned by <paramref name="binder"/> or the original failure.</returns>
    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder, string? traceId = null)
    {
        if (binder is null)
        {
            throw new ArgumentNullException(nameof(binder));
        }
        return IsFailure ? Result.Failure<TResult>(Errors, traceId ?? TraceId) : binder(Value);
    }

    /// <summary>
    /// Executes a side-effect when the result is successful, returning the original result.
    /// </summary>
    /// <param name="tap">Action to execute on the value.</param>
    /// <returns>The original result.</returns>
    public Result<T> Tap(Action<T> tap)
    {
        if (tap is null)
        {
            throw new ArgumentNullException(nameof(tap));
        }
        if (IsSuccess)
        {
            tap(Value);
        }

        return this;
    }

    /// <summary>
    /// Ensures the contained value satisfies the provided predicate or returns a failure.
    /// </summary>
    /// <param name="predicate">Predicate to validate the value.</param>
    /// <param name="error">Error to return on failure.</param>
    /// <returns>The original result when predicate passes; otherwise a failure result.</returns>
    public Result<T> Ensure(Func<T, bool> predicate, Error error)
    {
        if (predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        if (IsFailure)
        {
            return this;
        }

        return predicate(Value) ? this : Failure(error, TraceId);
    }

    /// <summary>
    /// Pattern matches the result, executing the appropriate delegate based on success or failure.
    /// </summary>
    /// <typeparam name="TResult">Return type from the match.</typeparam>
    /// <param name="onSuccess">Handler invoked when result is successful.</param>
    /// <param name="onFailure">Handler invoked when result is a failure.</param>
    /// <returns>The value produced by the invoked handler.</returns>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<IReadOnlyList<Error>, TResult> onFailure)
    {
        if (onSuccess is null)
        {
            throw new ArgumentNullException(nameof(onSuccess));
        }

        if (onFailure is null)
        {
            throw new ArgumentNullException(nameof(onFailure));
        }

        return IsSuccess ? onSuccess(Value) : onFailure(Errors);
    }
}
