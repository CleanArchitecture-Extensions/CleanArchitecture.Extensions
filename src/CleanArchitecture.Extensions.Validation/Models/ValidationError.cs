using System.Globalization;
using CleanArchitecture.Extensions.Core.Results;
using CleanArchitecture.Extensions.Validation.Options;
using FluentValidation.Results;

namespace CleanArchitecture.Extensions.Validation.Models;

/// <summary>
/// Represents a validation failure with optional property, attempted value, and metadata.
/// </summary>
public sealed record ValidationError
{
    private static readonly IReadOnlyDictionary<string, string> EmptyMetadata = new Dictionary<string, string>();

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationError"/> record.
    /// </summary>
    /// <param name="code">Error code associated with the validation failure.</param>
    /// <param name="message">Human-readable message describing the failure.</param>
    /// <param name="propertyName">Property name involved in the failure, if applicable.</param>
    /// <param name="attemptedValue">Attempted value as a string, if captured.</param>
    /// <param name="severity">Severity reported by the validator.</param>
    /// <param name="metadata">Optional metadata for diagnostics.</param>
    public ValidationError(string code, string message, string? propertyName = null, string? attemptedValue = null, string? severity = null, IReadOnlyDictionary<string, string>? metadata = null)
    {
        Code = string.IsNullOrWhiteSpace(code) ? "validation.failure" : code;
        Message = message ?? string.Empty;
        PropertyName = propertyName;
        AttemptedValue = attemptedValue;
        Severity = severity;
        Metadata = metadata ?? EmptyMetadata;
    }

    /// <summary>
    /// Gets the error code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the validation message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the property name if available.
    /// </summary>
    public string? PropertyName { get; }

    /// <summary>
    /// Gets the attempted value as a string, if available.
    /// </summary>
    public string? AttemptedValue { get; }

    /// <summary>
    /// Gets the severity reported by the validator.
    /// </summary>
    public string? Severity { get; }

    /// <summary>
    /// Gets metadata associated with the failure.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }

    /// <summary>
    /// Converts this validation error to a core <see cref="Error"/> with merged metadata.
    /// </summary>
    /// <param name="traceId">Optional trace identifier to attach.</param>
    /// <returns>An <see cref="Error"/> representing this validation failure.</returns>
    public Error ToCoreError(string? traceId = null)
    {
        var metadata = new Dictionary<string, string>(Metadata);

        if (!string.IsNullOrWhiteSpace(PropertyName))
        {
            metadata.TryAdd("property", PropertyName!);
        }

        if (!string.IsNullOrWhiteSpace(AttemptedValue))
        {
            metadata.TryAdd("attemptedValue", AttemptedValue!);
        }

        if (!string.IsNullOrWhiteSpace(Severity))
        {
            metadata.TryAdd("severity", Severity!);
        }

        return new Error(Code, Message, traceId, metadata);
    }

    /// <summary>
    /// Creates a <see cref="ValidationError"/> from a FluentValidation <see cref="ValidationFailure"/>.
    /// </summary>
    /// <param name="failure">Validation failure to transform.</param>
    /// <param name="options">Options controlling how metadata is captured.</param>
    /// <returns>A mapped <see cref="ValidationError"/>.</returns>
    public static ValidationError FromFailure(ValidationFailure failure, ValidationOptions options)
    {
        if (failure is null)
        {
            throw new ArgumentNullException(nameof(failure));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var metadata = options.IncludePlaceholderValues && failure.FormattedMessagePlaceholderValues is not null
            ? failure.FormattedMessagePlaceholderValues.ToDictionary(kvp => kvp.Key, kvp => FormatValue(kvp.Value))
            : new Dictionary<string, string>();

        var code = options.ErrorCodeSelector?.Invoke(failure);
        code = string.IsNullOrWhiteSpace(code) ? failure.ErrorCode : code;
        code = string.IsNullOrWhiteSpace(code) ? options.DefaultErrorCode : code;

        var message = options.MessageFormatter?.Invoke(failure) ?? failure.ErrorMessage ?? string.Empty;
        var attemptedValue = options.IncludeAttemptedValue ? FormatValue(failure.AttemptedValue) : null;
        var property = options.IncludePropertyName ? failure.PropertyName : null;
        var severity = failure.Severity.ToString();

        return new ValidationError(code!, message, property, attemptedValue, severity, metadata);
    }

    private static string FormatValue(object? value) =>
        value switch
        {
            null => string.Empty,
            DateTime dt => dt.ToString("O", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? value.ToString() ?? string.Empty,
            _ => value.ToString() ?? string.Empty
        };
}
