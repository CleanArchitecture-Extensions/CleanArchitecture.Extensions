using CleanArchitecture.Extensions.Multitenancy.Abstractions;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;

/// <summary>
/// Exposes tenant context information from a DbContext.
/// </summary>
public interface ITenantDbContext
{
    /// <summary>
    /// Gets the current tenant identifier.
    /// </summary>
    string? CurrentTenantId { get; }

    /// <summary>
    /// Gets the current tenant metadata.
    /// </summary>
    ITenantInfo? CurrentTenantInfo { get; }
}
