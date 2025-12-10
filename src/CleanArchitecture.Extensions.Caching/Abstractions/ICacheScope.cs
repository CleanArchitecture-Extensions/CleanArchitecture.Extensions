using CleanArchitecture.Extensions.Caching.Keys;

namespace CleanArchitecture.Extensions.Caching.Abstractions;

/// <summary>
/// Represents a cache scope that carries namespace and tenant context for key generation.
/// </summary>
public interface ICacheScope
{
    /// <summary>
    /// Gets the cache namespace applied to generated keys.
    /// </summary>
    string Namespace { get; }

    /// <summary>
    /// Gets the tenant identifier applied to generated keys, if any.
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// Builds a cache key for the given resource and parameter hash.
    /// </summary>
    CacheKey Create(string resource, string hash);

    /// <summary>
    /// Builds a cache key for the given request type and parameter hash.
    /// </summary>
    /// <typeparam name="TRequest">Request type used as the resource component.</typeparam>
    CacheKey CreateForRequest<TRequest>(string hash);
}
