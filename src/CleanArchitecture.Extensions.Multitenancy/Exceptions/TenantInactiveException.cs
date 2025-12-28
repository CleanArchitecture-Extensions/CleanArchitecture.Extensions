namespace CleanArchitecture.Extensions.Multitenancy.Exceptions;

/// <summary>
/// Indicates that a tenant is inactive.
/// </summary>
public sealed class TenantInactiveException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TenantInactiveException"/> class.
    /// </summary>
    /// <param name="tenantId">Tenant identifier.</param>
    public TenantInactiveException(string? tenantId)
        : base(BuildMessage(tenantId))
    {
        TenantId = tenantId;
    }

    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    public string? TenantId { get; }

    private static string BuildMessage(string? tenantId) =>
        string.IsNullOrWhiteSpace(tenantId)
            ? "Tenant is inactive."
            : $"Tenant '{tenantId}' is inactive.";
}
