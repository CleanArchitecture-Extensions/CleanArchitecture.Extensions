using CleanArchitecture.Extensions.Core.Results;

namespace CleanArchitecture.Extensions.Core.Guards;

/// <summary>
/// Provides guard clause helpers for validating inputs and state.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Ensures the input is not null.
    /// </summary>
    /// <typeparam name="T">Type of the input.</typeparam>
    /// <param name="input">Input value to validate.</param>
    /// <param name="parameterName">Parameter name used for error messages.</param>
    /// <param name="options">Guard behavior options.</param>
    /// <returns>A successful result when input is not null; otherwise a failure.</returns>
    public static Result<T> AgainstNull<T>(T? input, string parameterName, GuardOptions? options = null)
    {
        if (input is not null)
        {
            return Result.Success(input, options?.TraceId);
        }

        return HandleFailure<T>(CreateError("guard.null", $"{parameterName} cannot be null.", options), options, parameterName);
    }

    /// <summary>
    /// Ensures the string input is neither null nor whitespace.
    /// </summary>
    /// <param name="input">Input string to validate.</param>
    /// <param name="parameterName">Parameter name used for error messages.</param>
    /// <param name="options">Guard behavior options.</param>
    /// <returns>A successful result when input is present; otherwise a failure.</returns>
    public static Result<string> AgainstNullOrWhiteSpace(string? input, string parameterName, GuardOptions? options = null)
    {
        if (!string.IsNullOrWhiteSpace(input))
        {
            return Result.Success(input!, options?.TraceId);
        }

        return HandleFailure<string>(CreateError("guard.empty", $"{parameterName} cannot be null or whitespace.", options), options, parameterName);
    }

    /// <summary>
    /// Ensures the input value is within the specified range.
    /// </summary>
    /// <typeparam name="T">Comparable type for the input.</typeparam>
    /// <param name="value">Value to validate.</param>
    /// <param name="minimum">Minimum allowed value.</param>
    /// <param name="maximum">Maximum allowed value.</param>
    /// <param name="parameterName">Parameter name used for error messages.</param>
    /// <param name="options">Guard behavior options.</param>
    /// <returns>A successful result when value is within range; otherwise a failure.</returns>
    public static Result<T> AgainstOutOfRange<T>(T value, T minimum, T maximum, string parameterName, GuardOptions? options = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(minimum) >= 0 && value.CompareTo(maximum) <= 0)
        {
            return Result.Success(value, options?.TraceId);
        }

        return HandleFailure<T>(CreateError("guard.range", $"{parameterName} must be between {minimum} and {maximum}.", options), options, parameterName);
    }

    /// <summary>
    /// Ensures the provided enum value is defined.
    /// </summary>
    /// <typeparam name="TEnum">Enumeration type.</typeparam>
    /// <param name="value">Enum value to validate.</param>
    /// <param name="parameterName">Parameter name used for error messages.</param>
    /// <param name="options">Guard behavior options.</param>
    /// <returns>A successful result when value is defined; otherwise a failure.</returns>
    public static Result<TEnum> AgainstUndefinedEnum<TEnum>(TEnum value, string parameterName, GuardOptions? options = null)
        where TEnum : struct, Enum
    {
        if (Enum.IsDefined(value))
        {
            return Result.Success(value, options?.TraceId);
        }

        return HandleFailure<TEnum>(CreateError("guard.enum", $"{parameterName} is not a defined {typeof(TEnum).Name} value.", options), options, parameterName);
    }

    /// <summary>
    /// Ensures the input string meets the minimum length requirement.
    /// </summary>
    /// <param name="input">Input string to validate.</param>
    /// <param name="minLength">Minimum length allowed.</param>
    /// <param name="parameterName">Parameter name used for error messages.</param>
    /// <param name="options">Guard behavior options.</param>
    /// <returns>A successful result when input length is valid; otherwise a failure.</returns>
    public static Result<string> AgainstTooShort(string input, int minLength, string parameterName, GuardOptions? options = null)
    {
        if (input.Length >= minLength)
        {
            return Result.Success(input, options?.TraceId);
        }

        return HandleFailure<string>(CreateError("guard.length", $"{parameterName} must be at least {minLength} characters long.", options), options, parameterName);
    }

    /// <summary>
    /// Ensures the input string does not exceed the maximum length.
    /// </summary>
    /// <param name="input">Input string to validate.</param>
    /// <param name="maxLength">Maximum length allowed.</param>
    /// <param name="parameterName">Parameter name used for error messages.</param>
    /// <param name="options">Guard behavior options.</param>
    /// <returns>A successful result when input length is valid; otherwise a failure.</returns>
    public static Result<string> AgainstTooLong(string input, int maxLength, string parameterName, GuardOptions? options = null)
    {
        if (input.Length <= maxLength)
        {
            return Result.Success(input, options?.TraceId);
        }

        return HandleFailure<string>(CreateError("guard.length", $"{parameterName} must be {maxLength} characters or fewer.", options), options, parameterName);
    }

    /// <summary>
    /// Ensures the provided condition is true.
    /// </summary>
    /// <param name="condition">Condition to evaluate.</param>
    /// <param name="code">Error code when the condition fails.</param>
    /// <param name="message">Error message when the condition fails.</param>
    /// <param name="options">Guard behavior options.</param>
    /// <returns>A successful result when condition is true; otherwise a failure.</returns>
    public static Result Ensure(bool condition, string code, string message, GuardOptions? options = null)
    {
        return condition
            ? Result.Success(options?.TraceId)
            : HandleFailure(CreateError(code, message, options), options);
    }

    /// <summary>
    /// Handles guard failures for non-generic results based on configured strategy.
    /// </summary>
    /// <param name="error">Error describing the guard failure.</param>
    /// <param name="options">Guard behavior options.</param>
    /// <param name="parameterName">Name of the parameter used when throwing exceptions.</param>
    /// <returns>A failure result when not throwing.</returns>
    private static Result HandleFailure(Error error, GuardOptions? options, string? parameterName = null)
    {
        var strategy = options?.Strategy ?? GuardStrategy.ReturnFailure;
        switch (strategy)
        {
            case GuardStrategy.Throw:
                throw options?.ExceptionFactory?.Invoke(error) ?? new ArgumentException(error.Message, parameterName);
            case GuardStrategy.Accumulate:
                options?.ErrorSink?.Add(error);
                return Result.Failure(error, error.TraceId);
            default:
                return Result.Failure(error, error.TraceId);
        }
    }

    /// <summary>
    /// Handles guard failures for generic results based on configured strategy.
    /// </summary>
    /// <typeparam name="T">Type of the successful result value.</typeparam>
    /// <param name="error">Error describing the guard failure.</param>
    /// <param name="options">Guard behavior options.</param>
    /// <param name="parameterName">Name of the parameter used when throwing exceptions.</param>
    /// <returns>A failure result when not throwing.</returns>
    private static Result<T> HandleFailure<T>(Error error, GuardOptions? options, string? parameterName = null)
    {
        var strategy = options?.Strategy ?? GuardStrategy.ReturnFailure;
        switch (strategy)
        {
            case GuardStrategy.Throw:
                throw options?.ExceptionFactory?.Invoke(error) ?? new ArgumentException(error.Message, parameterName);
            case GuardStrategy.Accumulate:
                options?.ErrorSink?.Add(error);
                return Result.Failure<T>(error, error.TraceId);
            default:
                return Result.Failure<T>(error, error.TraceId);
        }
    }

    /// <summary>
    /// Creates an error instance using provided metadata and options.
    /// </summary>
    /// <param name="code">Error code.</param>
    /// <param name="message">Error message.</param>
    /// <param name="options">Guard behavior options.</param>
    /// <returns>An <see cref="Error"/> containing code, message, and trace ID.</returns>
    private static Error CreateError(string code, string message, GuardOptions? options)
    {
        return new Error(code, message, options?.TraceId);
    }
}
