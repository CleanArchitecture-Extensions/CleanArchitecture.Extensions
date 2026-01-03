using CleanArchitecture.Extensions.Caching.Abstractions;
using CleanArchitecture.Extensions.Caching;

namespace CleanArchitecture.Extensions.Caching.Options;

/// <summary>
/// Options controlling query caching behavior.
/// </summary>
public sealed class QueryCachingBehaviorOptions
{
    /// <summary>
    /// Gets or sets a predicate used to determine whether a request should be cached.
    /// </summary>
    public Func<object, bool> CachePredicate { get; set; } = request =>
        request is ICacheableQuery ||
        (request is not null && Attribute.IsDefined(request.GetType(), typeof(CacheableQueryAttribute), inherit: true));

    /// <summary>
    /// Gets or sets a delegate to derive the resource name used in cache keys.
    /// </summary>
    public Func<object, string>? ResourceNameSelector { get; set; }

    /// <summary>
    /// Gets or sets a delegate to generate a hash for the request.
    /// </summary>
    public Func<object, string>? HashFactory { get; set; }

    /// <summary>
    /// Gets or sets the default TTL applied to cached queries when not overridden per request type.
    /// </summary>
    public TimeSpan? DefaultTtl { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the map of per-request-type TTL overrides.
    /// </summary>
    public Dictionary<Type, TimeSpan> TtlByRequestType { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether null responses are cached.
    /// </summary>
    public bool CacheNullValues { get; set; } = true;

    /// <summary>
    /// Gets or sets an optional predicate to decide whether a response should be cached.
    /// </summary>
    public Func<object, object?, bool>? ResponseCachePredicate { get; set; }

    internal bool ShouldCache(object request) => CachePredicate?.Invoke(request) ?? false;

    internal bool ShouldCacheResponse(object request, object? response)
    {
        if (!CacheNullValues && response is null)
        {
            return false;
        }

        return ResponseCachePredicate?.Invoke(request, response) ?? true;
    }
}
