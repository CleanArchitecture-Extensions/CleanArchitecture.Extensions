namespace CleanArchitecture.Extensions.Caching.Options;

/// <summary>
/// Configures caching defaults for the extensions package.
/// </summary>
public sealed class CachingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether caching is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the default namespace applied to cache keys to avoid collisions across applications.
    /// </summary>
    public string DefaultNamespace { get; set; } = "CleanArchitectureExtensions";

    /// <summary>
    /// Gets or sets the default entry options applied when callers do not specify overrides.
    /// </summary>
    public CacheEntryOptions DefaultEntryOptions { get; set; } = CacheEntryOptions.Default;

    /// <summary>
    /// Gets or sets the default stampede mitigation policy.
    /// </summary>
    public CacheStampedePolicy StampedePolicy { get; set; } = CacheStampedePolicy.Default;

    /// <summary>
    /// Gets or sets the maximum allowed size for cache entries (in bytes) when providers support enforcement.
    /// </summary>
    public long? MaxEntrySizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the preferred serializer type name or content type to use when multiple serializers are registered.
    /// </summary>
    public string? PreferredSerializer { get; set; }

    /// <summary>
    /// Gets the default options instance.
    /// </summary>
    public static CachingOptions Default => new();
}
