using System.Globalization;
using System.Net;
using CleanArchitecture.Extensions.Core.Results;
using CleanArchitecture.Extensions.Exceptions.BaseTypes;
using CleanArchitecture.Extensions.Exceptions.Redaction;

namespace CleanArchitecture.Extensions.Exceptions.Catalog;

/// <summary>
/// Describes how an exception maps to a stable code, message, and optional HTTP status.
/// </summary>
public sealed class ExceptionDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionDescriptor"/> class.
    /// </summary>
    /// <param name="exceptionType">Exception type covered by this descriptor.</param>
    /// <param name="code">Stable error code.</param>
    /// <param name="message">Default message.</param>
    /// <param name="severity">Severity classification.</param>
    /// <param name="isTransient">Indicates whether the exception is transient/retryable.</param>
    /// <param name="statusCode">Optional HTTP status hint.</param>
    /// <param name="metadata">Optional metadata for diagnostics.</param>
    public ExceptionDescriptor(
        Type exceptionType,
        string code,
        string message,
        ExceptionSeverity severity = ExceptionSeverity.Error,
        bool isTransient = false,
        HttpStatusCode? statusCode = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ExceptionType = exceptionType ?? throw new ArgumentNullException(nameof(exceptionType));
        Code = string.IsNullOrWhiteSpace(code) ? ExceptionCodes.Unknown : code;
        Message = string.IsNullOrWhiteSpace(message) ? "An error occurred." : message;
        Severity = severity;
        IsTransient = isTransient;
        StatusCode = statusCode;
        Metadata = metadata ?? new Dictionary<string, string>();
    }

    /// <summary>
    /// Gets the exception type this descriptor covers.
    /// </summary>
    public Type ExceptionType { get; }

    /// <summary>
    /// Gets the stable error code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the default message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the severity classification.
    /// </summary>
    public ExceptionSeverity Severity { get; }

    /// <summary>
    /// Gets a value indicating whether the exception is transient/retryable.
    /// </summary>
    public bool IsTransient { get; }

    /// <summary>
    /// Gets the HTTP status hint, if any.
    /// </summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// Gets descriptor metadata used for diagnostics.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }

    /// <summary>
    /// Creates an error representation for the given exception.
    /// </summary>
    /// <param name="exception">Exception to map.</param>
    /// <param name="traceId">Trace identifier to attach.</param>
    /// <param name="includeExceptionDetails">Whether to flow exception messages or default catalog messages.</param>
    /// <param name="redactSensitiveData">Whether to redact sensitive data.</param>
    /// <param name="redactor">Redactor used to scrub values.</param>
    /// <returns>Structured error.</returns>
    public Error ToError(Exception exception, string? traceId, bool includeExceptionDetails, bool redactSensitiveData, ExceptionRedactor redactor)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        if (redactor is null)
        {
            throw new ArgumentNullException(nameof(redactor));
        }

        var message = includeExceptionDetails
            ? exception.Message
            : Message;

        if (string.IsNullOrWhiteSpace(message))
        {
            message = Message;
        }

        if (redactSensitiveData)
        {
            message = redactor.Redact(message);
        }

        var metadata = new Dictionary<string, string>(Metadata, StringComparer.OrdinalIgnoreCase)
        {
            ["exceptionType"] = exception.GetType().FullName ?? exception.GetType().Name,
            ["severity"] = Severity.ToString()
        };

        if (StatusCode.HasValue)
        {
            metadata["status"] = ((int)StatusCode.Value).ToString(CultureInfo.InvariantCulture);
        }

        if (IsTransient)
        {
            metadata["transient"] = bool.TrueString;
        }

        var finalMetadata = redactSensitiveData ? redactor.RedactMetadata(metadata) : metadata;
        return new Error(Code, message, traceId, finalMetadata);
    }

    /// <summary>
    /// Creates a descriptor from an <see cref="ApplicationExceptionBase"/> instance.
    /// </summary>
    /// <param name="exception">Exception carrying codes and metadata.</param>
    /// <returns>Descriptor derived from the exception data.</returns>
    public static ExceptionDescriptor FromApplicationException(ApplicationExceptionBase exception)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        return new ExceptionDescriptor(
            exception.GetType(),
            exception.Code,
            exception.Message,
            exception.Severity,
            exception.IsTransient,
            exception.StatusCode,
            exception.Metadata);
    }
}
