namespace CleanArchitecture.Extensions.Multitenancy.Exceptions;

/// <summary>
/// Indicates that a tenant is suspended.
/// </summary>
public sealed class TenantSuspendedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TenantSuspendedException"/> class.
    /// </summary>
    /// <param name="tenantId">Tenant identifier.</param>
    public TenantSuspendedException(string? tenantId)
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
            ? "Tenant is suspended."
            : $"Tenant '{tenantId}' is suspended.";
}
