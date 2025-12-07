using System.Net;

namespace CleanArchitecture.Extensions.Exceptions.BaseTypes;

/// <summary>
/// Represents a forbidden access violation.
/// </summary>
public class ForbiddenException : ApplicationExceptionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    public ForbiddenException()
        : this("Forbidden.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class with a custom message.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public ForbiddenException(string message)
        : base(ExceptionCodes.Forbidden, message, ExceptionSeverity.Error, false, HttpStatusCode.Forbidden)
    {
    }
}
