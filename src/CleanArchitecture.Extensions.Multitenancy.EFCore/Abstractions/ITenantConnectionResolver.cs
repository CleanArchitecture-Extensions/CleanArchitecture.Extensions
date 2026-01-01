using CleanArchitecture.Extensions.Multitenancy.Abstractions;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;

/// <summary>
/// Resolves tenant-specific connection strings.
/// </summary>
public interface ITenantConnectionResolver
{
    /// <summary>
    /// Resolves a connection string for the specified tenant.
    /// </summary>
    /// <param name="tenant">Tenant metadata.</param>
    string? ResolveConnectionString(ITenantInfo? tenant);
}
