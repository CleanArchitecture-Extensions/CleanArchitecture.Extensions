namespace CleanArchitecture.Extensions.Multitenancy.Exceptions;

/// <summary>
/// Indicates that a tenant could not be resolved.
/// </summary>
public sealed class TenantNotResolvedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TenantNotResolvedException"/> class.
    /// </summary>
    public TenantNotResolvedException()
        : base("Tenant could not be resolved.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantNotResolvedException"/> class with a custom message.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public TenantNotResolvedException(string? message)
        : base(string.IsNullOrWhiteSpace(message) ? "Tenant could not be resolved." : message)
    {
    }
}
