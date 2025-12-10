namespace CleanArchitecture.Extensions.Caching.Keys;

/// <summary>
/// Factory for producing deterministic cache keys.
/// </summary>
public interface ICacheKeyFactory
{
    /// <summary>
    /// Normalizes and constructs a cache key from components.
    /// </summary>
    /// <param name="resource">Resource name (e.g., request type or logical identifier).</param>
    /// <param name="hash">Hashed representation of parameters.</param>
    /// <param name="tenantId">Optional tenant identifier.</param>
    /// <param name="cacheNamespace">Optional namespace override.</param>
    /// <returns>A <see cref="CacheKey"/>.</returns>
    CacheKey Create(string resource, string hash, string? tenantId = null, string? cacheNamespace = null);

    /// <summary>
    /// Builds a cache key using a request type name as the resource.
    /// </summary>
    /// <typeparam name="TRequest">Request type used as the resource component.</typeparam>
    /// <param name="request">Request instance to hash.</param>
    /// <param name="tenantId">Optional tenant identifier.</param>
    /// <param name="cacheNamespace">Optional namespace override.</param>
    /// <returns>A <see cref="CacheKey"/>.</returns>
    CacheKey FromRequest<TRequest>(TRequest request, string? tenantId = null, string? cacheNamespace = null);

    /// <summary>
    /// Produces a deterministic hash for the provided parameters.
    /// </summary>
    /// <param name="parameters">Parameters to hash (e.g., request object).</param>
    /// <returns>A canonical hash string.</returns>
    string CreateHash(object? parameters);

    /// <summary>
    /// Normalizes the namespace component.
    /// </summary>
    /// <param name="cacheNamespace">Namespace to normalize.</param>
    /// <returns>A canonical namespace string.</returns>
    string NormalizeNamespace(string? cacheNamespace);
}
