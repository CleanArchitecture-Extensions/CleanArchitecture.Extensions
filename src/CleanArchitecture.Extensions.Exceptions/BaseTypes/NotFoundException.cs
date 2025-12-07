using System.Net;

namespace CleanArchitecture.Extensions.Exceptions.BaseTypes;

/// <summary>
/// Represents a missing resource.
/// </summary>
public class NotFoundException : ApplicationExceptionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    public NotFoundException()
        : this("The specified resource was not found.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a custom message.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public NotFoundException(string message)
        : base(ExceptionCodes.NotFound, message, ExceptionSeverity.Error, false, HttpStatusCode.NotFound)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class for a named resource.
    /// </summary>
    /// <param name="resourceName">Resource name.</param>
    /// <param name="key">Resource identifier.</param>
    public NotFoundException(string resourceName, object? key)
        : this($"The resource \"{resourceName}\" ({key}) was not found.")
    {
        ResourceName = resourceName;
        ResourceKey = key?.ToString();
    }

    /// <summary>
    /// Gets the resource name, when supplied.
    /// </summary>
    public string? ResourceName { get; }

    /// <summary>
    /// Gets the resource identifier, when supplied.
    /// </summary>
    public string? ResourceKey { get; }
}
