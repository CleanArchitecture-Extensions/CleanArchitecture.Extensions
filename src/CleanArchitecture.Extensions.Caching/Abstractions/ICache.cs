using CleanArchitecture.Extensions.Caching.Keys;
using CleanArchitecture.Extensions.Caching.Options;

namespace CleanArchitecture.Extensions.Caching.Abstractions;

/// <summary>
/// Provider-agnostic cache abstraction with synchronous and asynchronous APIs.
/// </summary>
public interface ICache
{
    /// <summary>
    /// Attempts to retrieve a cached item.
    /// </summary>
    CacheItem<T?>? Get<T>(CacheKey key);

    /// <summary>
    /// Attempts to retrieve a cached item asynchronously.
    /// </summary>
    Task<CacheItem<T?>?> GetAsync<T>(CacheKey key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds or replaces a cached item.
    /// </summary>
    void Set<T>(CacheKey key, T value, CacheEntryOptions? options = null);

    /// <summary>
    /// Adds or replaces a cached item asynchronously.
    /// </summary>
    Task SetAsync<T>(CacheKey key, T value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a cached item if present; otherwise computes and stores a value.
    /// </summary>
    T? GetOrAdd<T>(CacheKey key, Func<T> factory, CacheEntryOptions? options = null, CacheStampedePolicy? stampedePolicy = null);

    /// <summary>
    /// Returns a cached item if present; otherwise computes and stores a value asynchronously.
    /// </summary>
    Task<T?> GetOrAddAsync<T>(CacheKey key, Func<CancellationToken, Task<T>> factory, CacheEntryOptions? options = null, CacheStampedePolicy? stampedePolicy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a cached item if present.
    /// </summary>
    void Remove(CacheKey key);

    /// <summary>
    /// Removes a cached item asynchronously if present.
    /// </summary>
    Task RemoveAsync(CacheKey key, CancellationToken cancellationToken = default);
}
