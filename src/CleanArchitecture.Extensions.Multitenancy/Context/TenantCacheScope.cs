using CleanArchitecture.Extensions.Caching.Abstractions;
using CleanArchitecture.Extensions.Caching.Keys;
using CleanArchitecture.Extensions.Caching.Options;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.Context;

/// <summary>
/// Cache scope that binds cache keys to the current tenant context.
/// </summary>
public sealed class TenantCacheScope : ICacheScope
{
    private readonly ICurrentTenant _currentTenant;
    private readonly ICacheKeyFactory _keyFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantCacheScope"/> class.
    /// </summary>
    /// <param name="currentTenant">Current tenant accessor.</param>
    /// <param name="keyFactory">Cache key factory.</param>
    /// <param name="options">Caching options.</param>
    public TenantCacheScope(
        ICurrentTenant currentTenant,
        ICacheKeyFactory keyFactory,
        IOptions<CachingOptions> options)
    {
        _currentTenant = currentTenant ?? throw new ArgumentNullException(nameof(currentTenant));
        _keyFactory = keyFactory ?? throw new ArgumentNullException(nameof(keyFactory));
        ArgumentNullException.ThrowIfNull(options);

        Namespace = _keyFactory.NormalizeNamespace(options.Value.DefaultNamespace);
    }

    /// <inheritdoc />
    public string Namespace { get; }

    /// <inheritdoc />
    public string? TenantId => _currentTenant.TenantId;

    /// <inheritdoc />
    public CacheKey Create(string resource, string hash) => _keyFactory.Create(resource, hash, TenantId, Namespace);

    /// <inheritdoc />
    public CacheKey CreateForRequest<TRequest>(string hash) => Create(typeof(TRequest).Name, hash);
}
