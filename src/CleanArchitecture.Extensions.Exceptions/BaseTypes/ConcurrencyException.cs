using System.Net;

namespace CleanArchitecture.Extensions.Exceptions.BaseTypes;

/// <summary>
/// Represents a concurrency conflict, typically retryable.
/// </summary>
public class ConcurrencyException : ApplicationExceptionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
    /// </summary>
    public ConcurrencyException()
        : this("A concurrency conflict occurred.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyException"/> class with a custom message.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public ConcurrencyException(string message)
        : base(ExceptionCodes.Concurrency, message, ExceptionSeverity.Warning, true, HttpStatusCode.Conflict)
    {
    }
}
