using System.Net;

namespace CleanArchitecture.Extensions.Exceptions.BaseTypes;

/// <summary>
/// Represents a transient failure that can typically be retried.
/// </summary>
public class TransientException : ApplicationExceptionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransientException"/> class.
    /// </summary>
    public TransientException()
        : this("A transient failure occurred.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TransientException"/> class with a custom message.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public TransientException(string message)
        : base(ExceptionCodes.Transient, message, ExceptionSeverity.Warning, true, HttpStatusCode.ServiceUnavailable)
    {
    }
}
