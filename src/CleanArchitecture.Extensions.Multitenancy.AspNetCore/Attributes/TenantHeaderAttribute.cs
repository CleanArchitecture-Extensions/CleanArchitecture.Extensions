namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Attributes;

/// <summary>
/// Describes the tenant header used by an endpoint.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
public sealed class TenantHeaderAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TenantHeaderAttribute"/> class.
    /// </summary>
    /// <param name="headerName">Tenant header name.</param>
    public TenantHeaderAttribute(string headerName)
    {
        if (string.IsNullOrWhiteSpace(headerName))
        {
            throw new ArgumentException("Header name cannot be empty.", nameof(headerName));
        }

        HeaderName = headerName;
    }

    /// <summary>
    /// Gets the tenant header name.
    /// </summary>
    public string HeaderName { get; }
}
