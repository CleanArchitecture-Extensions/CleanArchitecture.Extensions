using System.Net;

namespace CleanArchitecture.Extensions.Exceptions.BaseTypes;

/// <summary>
/// Represents an unauthorized access attempt.
/// </summary>
public class UnauthorizedException : ApplicationExceptionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
    /// </summary>
    public UnauthorizedException()
        : this("Unauthorized.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class with a custom message.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public UnauthorizedException(string message)
        : base(ExceptionCodes.Unauthorized, message, ExceptionSeverity.Error, false, HttpStatusCode.Unauthorized)
    {
    }
}
