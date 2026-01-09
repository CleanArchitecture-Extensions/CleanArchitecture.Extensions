using CleanArchitecture.Extensions.Caching.Keys;

namespace CleanArchitecture.Extensions.Caching.Abstractions;

/// <summary>
/// Provides a custom cache hash for requests that cannot be deterministically serialized.
/// </summary>
public interface ICacheKeyProvider
{
    /// <summary>
    /// Returns a deterministic hash used to build cache keys.
    /// </summary>
    /// <param name="keyFactory">Cache key factory to help create component hashes.</param>
    /// <returns>A canonical hash string.</returns>
    string GetCacheHash(ICacheKeyFactory keyFactory);
}
