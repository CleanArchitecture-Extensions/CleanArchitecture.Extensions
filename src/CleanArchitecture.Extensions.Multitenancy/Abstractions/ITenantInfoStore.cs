namespace CleanArchitecture.Extensions.Multitenancy.Abstractions;

/// <summary>
/// Provides tenant metadata lookup from a persistent store.
/// </summary>
public interface ITenantInfoStore
{
    /// <summary>
    /// Finds a tenant by identifier.
    /// </summary>
    /// <param name="tenantId">Tenant identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tenant info or null when not found.</returns>
    Task<ITenantInfo?> FindByIdAsync(string tenantId, CancellationToken cancellationToken = default);
}
