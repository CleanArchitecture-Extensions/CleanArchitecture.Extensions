using CleanArchitecture.Extensions.Caching.Abstractions;
using CleanArchitecture.Extensions.Caching.Options;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Caching.Keys;

/// <summary>
/// Default cache scope that applies a namespace and optional tenant when building keys.
/// </summary>
public sealed class DefaultCacheScope : ICacheScope
{
    private readonly ICacheKeyFactory _keyFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultCacheScope"/> class.
    /// </summary>
    /// <param name="keyFactory">Key factory used to build canonical keys.</param>
    /// <param name="options">Caching options.</param>
    /// <param name="tenantId">Optional tenant identifier.</param>
    public DefaultCacheScope(ICacheKeyFactory keyFactory, IOptions<CachingOptions> options, string? tenantId = null)
    {
        _keyFactory = keyFactory ?? throw new ArgumentNullException(nameof(keyFactory));
        ArgumentNullException.ThrowIfNull(options);

        Namespace = _keyFactory.NormalizeNamespace(options.Value.DefaultNamespace);
        TenantId = string.IsNullOrWhiteSpace(tenantId) ? null : tenantId.Trim();
    }

    /// <inheritdoc />
    public string Namespace { get; }

    /// <inheritdoc />
    public string? TenantId { get; }

    /// <inheritdoc />
    public CacheKey Create(string resource, string hash) => _keyFactory.Create(resource, hash, TenantId, Namespace);

    /// <inheritdoc />
    public CacheKey CreateForRequest<TRequest>(string hash) => Create(typeof(TRequest).Name, hash);
}
