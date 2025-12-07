using System.Net;

namespace CleanArchitecture.Extensions.Exceptions.BaseTypes;

/// <summary>
/// Represents a resource conflict.
/// </summary>
public class ConflictException : ApplicationExceptionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class.
    /// </summary>
    public ConflictException()
        : this("The request conflicts with the current state of the resource.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class with a custom message.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public ConflictException(string message)
        : base(ExceptionCodes.Conflict, message, ExceptionSeverity.Error, false, HttpStatusCode.Conflict)
    {
    }
}
