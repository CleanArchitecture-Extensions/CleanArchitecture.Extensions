namespace CleanArchitecture.Extensions.Caching.Keys;

/// <summary>
/// Represents a canonical cache key composed of namespace, tenant, resource, and hash segments.
/// </summary>
public readonly record struct CacheKey
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CacheKey"/> struct.
    /// </summary>
    /// <param name="cacheNamespace">Namespace segment.</param>
    /// <param name="resource">Resource segment.</param>
    /// <param name="hash">Hash segment derived from parameters.</param>
    /// <param name="tenantId">Optional tenant identifier.</param>
    /// <exception cref="ArgumentException">Thrown when required segments are null or whitespace.</exception>
    public CacheKey(string cacheNamespace, string resource, string hash, string? tenantId = null)
    {
        if (string.IsNullOrWhiteSpace(cacheNamespace))
        {
            throw new ArgumentException("Cache namespace cannot be null or whitespace.", nameof(cacheNamespace));
        }

        if (string.IsNullOrWhiteSpace(resource))
        {
            throw new ArgumentException("Resource cannot be null or whitespace.", nameof(resource));
        }

        if (string.IsNullOrWhiteSpace(hash))
        {
            throw new ArgumentException("Hash cannot be null or whitespace.", nameof(hash));
        }

        Namespace = cacheNamespace.Trim();
        Resource = resource.Trim();
        Hash = hash.Trim();
        TenantId = string.IsNullOrWhiteSpace(tenantId) ? null : tenantId.Trim();
    }

    /// <summary>
    /// Gets the namespace segment of the cache key.
    /// </summary>
    public string Namespace { get; }

    /// <summary>
    /// Gets the resource segment of the cache key (often the request type or logical resource name).
    /// </summary>
    public string Resource { get; }

    /// <summary>
    /// Gets the hashed parameters segment.
    /// </summary>
    public string Hash { get; }

    /// <summary>
    /// Gets the tenant identifier segment when applicable.
    /// </summary>
    public string? TenantId { get; }

    /// <summary>
    /// Gets the fully composed cache key string.
    /// </summary>
    public string FullKey => TenantId is null
        ? $"{Namespace}:{Resource}:{Hash}"
        : $"{Namespace}:{TenantId}:{Resource}:{Hash}";

    /// <summary>
    /// Returns the fully composed cache key string.
    /// </summary>
    public override string ToString() => FullKey;
}
