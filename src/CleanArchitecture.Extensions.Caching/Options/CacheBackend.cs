namespace CleanArchitecture.Extensions.Caching.Options;

/// <summary>
/// Defines the cache backend selection strategy.
/// </summary>
public enum CacheBackend
{
    /// <summary>
    /// Use a distributed cache when a non-memory implementation is available; otherwise fallback to memory.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// Always use the in-memory cache.
    /// </summary>
    Memory = 1,

    /// <summary>
    /// Always use the distributed cache.
    /// </summary>
    Distributed = 2
}
