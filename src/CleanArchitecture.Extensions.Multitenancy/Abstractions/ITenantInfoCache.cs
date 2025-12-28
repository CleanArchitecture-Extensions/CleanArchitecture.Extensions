namespace CleanArchitecture.Extensions.Multitenancy.Abstractions;

/// <summary>
/// Provides tenant metadata caching for resolution validation.
/// </summary>
public interface ITenantInfoCache
{
    /// <summary>
    /// Retrieves tenant metadata by identifier.
    /// </summary>
    /// <param name="tenantId">Tenant identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tenant info or null when not found.</returns>
    Task<ITenantInfo?> GetAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores tenant metadata in the cache.
    /// </summary>
    /// <param name="tenant">Tenant info to cache.</param>
    /// <param name="ttl">Time-to-live for the cache entry.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetAsync(ITenantInfo tenant, TimeSpan? ttl, CancellationToken cancellationToken = default);
}
