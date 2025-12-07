using System.Net;

namespace CleanArchitecture.Extensions.Exceptions.BaseTypes;

/// <summary>
/// Base exception type that carries structured error metadata for application and domain layers.
/// </summary>
public abstract class ApplicationExceptionBase : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationExceptionBase"/> class.
    /// </summary>
    /// <param name="code">Stable error code.</param>
    /// <param name="message">Human-friendly message.</param>
    /// <param name="severity">Severity classification.</param>
    /// <param name="isTransient">Indicates whether the error is retryable/transient.</param>
    /// <param name="statusCode">Optional HTTP status hint for transport layers.</param>
    /// <param name="innerException">Inner exception.</param>
    /// <param name="metadata">Optional metadata for diagnostics.</param>
    protected ApplicationExceptionBase(
        string code,
        string message,
        ExceptionSeverity severity = ExceptionSeverity.Error,
        bool isTransient = false,
        HttpStatusCode? statusCode = null,
        Exception? innerException = null,
        IReadOnlyDictionary<string, string>? metadata = null)
        : base(string.IsNullOrWhiteSpace(message) ? "An error occurred." : message, innerException)
    {
        Code = string.IsNullOrWhiteSpace(code) ? ExceptionCodes.Unknown : code;
        Severity = severity;
        IsTransient = isTransient;
        StatusCode = statusCode;
        Metadata = metadata ?? new Dictionary<string, string>();
    }

    /// <summary>
    /// Gets the stable error code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the severity classification.
    /// </summary>
    public ExceptionSeverity Severity { get; }

    /// <summary>
    /// Gets a value indicating whether the error is transient/retryable.
    /// </summary>
    public bool IsTransient { get; }

    /// <summary>
    /// Gets the HTTP status hint, if any.
    /// </summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// Gets metadata attached to the exception for diagnostics.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
