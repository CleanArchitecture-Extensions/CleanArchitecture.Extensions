using System.Diagnostics.CodeAnalysis;

namespace CleanArchitecture.Extensions.Core.Results;

/// <summary>
/// Compatibility result that mirrors the Jason Taylor template shape (Succeeded + string[] Errors) for drop-in use.
/// </summary>
public class LegacyResult
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool Succeeded { get; init; }

    /// <summary>
    /// Gets the error messages when the operation failed.
    /// </summary>
    public string[] Errors { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static LegacyResult Success() => new() { Succeeded = true };

    /// <summary>
    /// Creates a failed result with the provided errors.
    /// </summary>
    /// <param name="errors">Error messages describing the failure.</param>
    public static LegacyResult Failure(params string[] errors) =>
        new() { Succeeded = false, Errors = errors ?? Array.Empty<string>() };

    /// <summary>
    /// Creates a failed result with the provided errors.
    /// </summary>
    /// <param name="errors">Error messages describing the failure.</param>
    public static LegacyResult Failure(IEnumerable<string> errors) =>
        new() { Succeeded = false, Errors = errors?.ToArray() ?? Array.Empty<string>() };

    /// <summary>
    /// Creates a legacy result from a rich core <see cref="Result"/>, mapping errors with an optional formatter.
    /// </summary>
    /// <param name="result">Core result to convert.</param>
    /// <param name="errorFormatter">Optional formatter for converting <see cref="Error"/> to string messages.</param>
    public static LegacyResult FromResult(Result result, Func<Error, string>? errorFormatter = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        var formatter = errorFormatter ?? DefaultFormatter;
        return result.IsSuccess ? Success() : Failure(result.Errors.Select(formatter));
    }

    /// <summary>
    /// Converts this legacy result into a rich core <see cref="Result"/>.
    /// </summary>
    /// <param name="traceId">Optional trace identifier to attach to errors.</param>
    /// <param name="errorCode">Error code to apply when mapping string messages to <see cref="Error"/>.</param>
    public Result ToResult(string? traceId = null, string errorCode = "legacy.failure")
    {
        if (Succeeded)
        {
            return Result.Success(traceId);
        }

        var errors = Errors.Length == 0
            ? new[] { new Error(errorCode, "An error occurred.", traceId) }
            : Errors.Select(message => new Error(errorCode, message, traceId));

        return Result.Failure(errors, traceId);
    }

    /// <summary>
    /// Default mapping from structured <see cref="Error"/> to legacy string message.
    /// </summary>
    /// <param name="error">Error to map.</param>
    /// <returns>Error message when present; otherwise the error code.</returns>
    protected internal static string DefaultFormatter(Error error) =>
        string.IsNullOrWhiteSpace(error.Message) ? error.Code : error.Message;
}

/// <summary>
/// Compatibility result with value payload mirroring the Jason Taylor template shape.
/// </summary>
/// <typeparam name="T">Type of the value returned on success.</typeparam>
public sealed class LegacyResult<T> : LegacyResult
{
    /// <summary>
    /// Gets the value when the operation succeeds.
    /// </summary>
    [AllowNull]
    public T Value { get; init; } = default!;

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <param name="value">Value to return.</param>
    public static LegacyResult<T> Success(T value) => new() { Succeeded = true, Value = value };

    /// <summary>
    /// Creates a failed result with the provided errors.
    /// </summary>
    /// <param name="errors">Error messages describing the failure.</param>
    public new static LegacyResult<T> Failure(params string[] errors) =>
        new() { Succeeded = false, Errors = errors ?? Array.Empty<string>() };

    /// <summary>
    /// Creates a failed result with the provided errors.
    /// </summary>
    /// <param name="errors">Error messages describing the failure.</param>
    public new static LegacyResult<T> Failure(IEnumerable<string> errors) =>
        new() { Succeeded = false, Errors = errors?.ToArray() ?? Array.Empty<string>() };

    /// <summary>
    /// Creates a legacy result from a rich core <see cref="Result{T}"/>, mapping errors with an optional formatter.
    /// </summary>
    /// <param name="result">Core result to convert.</param>
    /// <param name="errorFormatter">Optional formatter for converting <see cref="Error"/> to string messages.</param>
    public static LegacyResult<T> FromResult(Result<T> result, Func<Error, string>? errorFormatter = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        var formatter = errorFormatter ?? DefaultFormatter;
        return result.IsSuccess
            ? Success(result.Value)
            : Failure(result.Errors.Select(formatter));
    }

    /// <summary>
    /// Converts this legacy result into a rich core <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="traceId">Optional trace identifier to attach to errors.</param>
    /// <param name="errorCode">Error code to apply when mapping string messages to <see cref="Error"/>.</param>
    public new Result<T> ToResult(string? traceId = null, string errorCode = "legacy.failure")
    {
        if (Succeeded)
        {
            return Result.Success(Value, traceId);
        }

        var errors = Errors.Length == 0
            ? new[] { new Error(errorCode, "An error occurred.", traceId) }
            : Errors.Select(message => new Error(errorCode, message, traceId));

        return Result.Failure<T>(errors, traceId);
    }
}
