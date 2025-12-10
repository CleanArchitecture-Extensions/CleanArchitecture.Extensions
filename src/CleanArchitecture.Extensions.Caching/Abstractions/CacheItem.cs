using CleanArchitecture.Extensions.Caching.Keys;
using CleanArchitecture.Extensions.Caching.Options;

namespace CleanArchitecture.Extensions.Caching.Abstractions;

/// <summary>
/// Represents a cached value and its metadata.
/// </summary>
/// <typeparam name="T">Type of the cached value.</typeparam>
public sealed class CacheItem<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CacheItem{T}"/> class.
    /// </summary>
    /// <param name="key">Cache key associated with the value.</param>
    /// <param name="value">Cached value.</param>
    /// <param name="createdAt">Timestamp when the item was stored.</param>
    /// <param name="expiresAt">Timestamp when the item should expire.</param>
    /// <param name="contentType">Optional content type used by the serializer.</param>
    /// <param name="options">Options used when storing the entry.</param>
    public CacheItem(CacheKey key, T? value, DateTimeOffset? createdAt = null, DateTimeOffset? expiresAt = null, string? contentType = null, CacheEntryOptions? options = null)
    {
        Key = key;
        Value = value;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        ContentType = contentType;
        Options = options;
    }

    /// <summary>
    /// Gets the key for the cached value.
    /// </summary>
    public CacheKey Key { get; }

    /// <summary>
    /// Gets the cached value.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Gets the timestamp when the item was created.
    /// </summary>
    public DateTimeOffset? CreatedAt { get; }

    /// <summary>
    /// Gets the timestamp when the item is expected to expire.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; }

    /// <summary>
    /// Gets the content type used by the serializer.
    /// </summary>
    public string? ContentType { get; }

    /// <summary>
    /// Gets the entry options that were applied when the item was stored.
    /// </summary>
    public CacheEntryOptions? Options { get; }
}
