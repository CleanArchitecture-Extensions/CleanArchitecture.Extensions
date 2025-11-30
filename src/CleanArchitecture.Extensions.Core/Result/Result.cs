using System.Collections.ObjectModel;

namespace CleanArchitecture.Extensions.Core.Results;

/// <summary>
/// Represents the outcome of an operation, capturing success state, trace information, and errors when applicable.
/// </summary>
public class Result
{
    private readonly IReadOnlyList<Error> _errors;

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation succeeded.</param>
    /// <param name="traceId">Optional trace identifier propagated through results.</param>
    /// <param name="errors">Collection of errors describing failures.</param>
    protected Result(bool isSuccess, string? traceId, IEnumerable<Error> errors)
    {
        IsSuccess = isSuccess;
        TraceId = traceId;
        _errors = new ReadOnlyCollection<Error>(errors.ToList());
    }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the trace identifier associated with this result, if any.
    /// </summary>
    public string? TraceId { get; }

    /// <summary>
    /// Gets the errors associated with the result.
    /// </summary>
    public IReadOnlyList<Error> Errors => _errors;

    /// <summary>
    /// Creates a successful result without a value payload.
    /// </summary>
    /// <param name="traceId">Optional trace identifier to attach.</param>
    /// <returns>A successful <see cref="Result"/>.</returns>
    public static Result Success(string? traceId = null) => new(true, traceId, Array.Empty<Error>());

    /// <summary>
    /// Creates a failed result with a single error.
    /// </summary>
    /// <param name="error">Error describing the failure.</param>
    /// <param name="traceId">Optional trace identifier to attach or override.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Failure(Error error, string? traceId = null)
    {
        var resolvedTraceId = traceId ?? error.TraceId;
        return new Result(false, resolvedTraceId, new[] { error.WithTraceId(resolvedTraceId) });
    }

    /// <summary>
    /// Creates a failed result with multiple errors.
    /// </summary>
    /// <param name="errors">Errors describing the failure.</param>
    /// <param name="traceId">Optional trace identifier to attach or override.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Failure(IEnumerable<Error> errors, string? traceId = null)
    {
        var errorList = errors.ToList();
        var resolvedTraceId = traceId ?? errorList.FirstOrDefault().TraceId;
        var normalizedErrors = errorList.Select(error => error.WithTraceId(resolvedTraceId)).ToList();
        return new Result(false, resolvedTraceId, normalizedErrors);
    }

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="value">Value to carry.</param>
    /// <param name="traceId">Optional trace identifier to attach.</param>
    /// <returns>A successful <see cref="Result{T}"/>.</returns>
    public static Result<T> Success<T>(T value, string? traceId = null) => Result<T>.Success(value, traceId);

    /// <summary>
    /// Creates a failed result with a single error and value type.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="error">Error describing the failure.</param>
    /// <param name="traceId">Optional trace identifier to attach or override.</param>
    /// <returns>A failed <see cref="Result{T}"/>.</returns>
    public static Result<T> Failure<T>(Error error, string? traceId = null) => Result<T>.Failure(error, traceId);

    /// <summary>
    /// Creates a failed result with multiple errors and value type.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="errors">Errors describing the failure.</param>
    /// <param name="traceId">Optional trace identifier to attach or override.</param>
    /// <returns>A failed <see cref="Result{T}"/>.</returns>
    public static Result<T> Failure<T>(IEnumerable<Error> errors, string? traceId = null) => Result<T>.Failure(errors, traceId);

    /// <summary>
    /// Combines multiple results into a single result, aggregating errors when any failure occurs.
    /// </summary>
    /// <param name="results">Results to combine.</param>
    /// <returns>A successful result if all inputs succeed; otherwise a failure containing aggregated errors.</returns>
    public static Result Combine(params Result[] results) => Combine(results.AsEnumerable());

    /// <summary>
    /// Combines an enumerable of results into a single result, aggregating errors when any failure occurs.
    /// </summary>
    /// <param name="results">Results to combine.</param>
    /// <returns>A successful result if all inputs succeed; otherwise a failure containing aggregated errors.</returns>
    public static Result Combine(IEnumerable<Result> results)
    {
        var resultList = results.ToList();
        if (resultList.Count == 0)
        {
            return Success();
        }

        var errors = new List<Error>();
        string? traceId = null;

        foreach (var result in resultList)
        {
            if (string.IsNullOrWhiteSpace(traceId) && !string.IsNullOrWhiteSpace(result.TraceId))
            {
                traceId = result.TraceId;
            }

            if (result.IsFailure)
            {
                errors.AddRange(result.Errors);
            }
        }

        return errors.Count == 0 ? Success(traceId) : Failure(errors, traceId);
    }
}
