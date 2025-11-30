namespace CleanArchitecture.Extensions.Core.Results;

/// <summary>
/// Extension helpers for composing and adapting <see cref="Result"/> instances.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Ensures a condition holds for a result; returns failure when predicate evaluates to false.
    /// </summary>
    /// <param name="result">Result to validate.</param>
    /// <param name="predicate">Predicate that must evaluate to true.</param>
    /// <param name="error">Error produced when predicate fails.</param>
    /// <returns>The original result when predicate passes; otherwise a failure result.</returns>
    public static Result Ensure(this Result result, Func<bool> predicate, Error error)
    {
        if (predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        if (result.IsFailure)
        {
            return result;
        }

        return predicate() ? result : Result.Failure(error, result.TraceId);
    }

    /// <summary>
    /// Wraps a value into a successful result.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="value">Value to wrap.</param>
    /// <param name="traceId">Optional trace identifier to attach.</param>
    /// <returns>A successful <see cref="Result{T}"/>.</returns>
    public static Result<T> ToResult<T>(this T value, string? traceId = null) => Result.Success(value, traceId);

    /// <summary>
    /// Recovers from a failed result using a fallback function.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="result">Result to inspect.</param>
    /// <param name="fallback">Function producing a fallback value from errors.</param>
    /// <returns>Original result when successful; otherwise a success result using the fallback value.</returns>
    public static Result<T> Recover<T>(this Result<T> result, Func<IReadOnlyList<Error>, T> fallback)
    {
        if (fallback is null)
        {
            throw new ArgumentNullException(nameof(fallback));
        }

        return result.IsSuccess ? result : Result.Success(fallback(result.Errors), result.TraceId);
    }
}
