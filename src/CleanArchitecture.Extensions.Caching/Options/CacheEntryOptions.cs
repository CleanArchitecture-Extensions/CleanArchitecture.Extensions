namespace CleanArchitecture.Extensions.Caching.Options;

/// <summary>
/// Represents cache entry configuration in a provider-agnostic shape.
/// </summary>
public sealed class CacheEntryOptions
{
    /// <summary>
    /// Gets or sets an absolute expiration relative to now.
    /// </summary>
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

    /// <summary>
    /// Gets or sets an absolute expiration timestamp.
    /// </summary>
    public DateTimeOffset? AbsoluteExpiration { get; set; }

    /// <summary>
    /// Gets or sets a sliding expiration interval.
    /// </summary>
    public TimeSpan? SlidingExpiration { get; set; }

    /// <summary>
    /// Gets or sets the cache priority used by providers that support eviction priority.
    /// </summary>
    public CachePriority Priority { get; set; } = CachePriority.Normal;

    /// <summary>
    /// Gets or sets the size hint for the entry when providers enforce size limits.
    /// </summary>
    public long? Size { get; set; }

    /// <summary>
    /// Gets the default entry options instance.
    /// </summary>
    public static CacheEntryOptions Default => new();
}
