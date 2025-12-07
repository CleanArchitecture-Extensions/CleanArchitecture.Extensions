namespace CleanArchitecture.Extensions.Exceptions.BaseTypes;

/// <summary>
/// Base exception for domain rule violations.
/// </summary>
public class DomainException : ApplicationExceptionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with the default domain code.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public DomainException(string message)
        : base(ExceptionCodes.Domain, message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with a custom code.
    /// </summary>
    /// <param name="code">Domain-specific error code.</param>
    /// <param name="message">Exception message.</param>
    /// <param name="innerException">Inner exception.</param>
    /// <param name="metadata">Optional metadata.</param>
    public DomainException(string code, string message, Exception? innerException = null, IReadOnlyDictionary<string, string>? metadata = null)
        : base(string.IsNullOrWhiteSpace(code) ? ExceptionCodes.Domain : code, message, ExceptionSeverity.Error, false, null, innerException, metadata)
    {
    }
}
