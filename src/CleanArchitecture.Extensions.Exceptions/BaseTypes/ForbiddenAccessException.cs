namespace CleanArchitecture.Extensions.Exceptions.BaseTypes;

/// <summary>
/// Compatibility type mirroring Jason Taylor's template for authorization failures.
/// </summary>
public class ForbiddenAccessException : ForbiddenException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenAccessException"/> class.
    /// </summary>
    public ForbiddenAccessException()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenAccessException"/> class with a custom message.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public ForbiddenAccessException(string message)
        : base(message)
    {
    }
}
