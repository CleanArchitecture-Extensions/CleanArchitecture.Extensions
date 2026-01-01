namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;

/// <summary>
/// Represents an entity that belongs to a tenant.
/// </summary>
public interface ITenantEntity
{
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    string TenantId { get; set; }
}
