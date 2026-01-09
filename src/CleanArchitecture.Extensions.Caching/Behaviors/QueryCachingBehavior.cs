using CleanArchitecture.Extensions.Caching.Abstractions;
using CleanArchitecture.Extensions.Caching.Internal;
using CleanArchitecture.Extensions.Caching.Keys;
using CleanArchitecture.Extensions.Caching.Options;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Caching.Behaviors;

/// <summary>
/// MediatR behavior that applies cache-aside semantics for queries.
/// </summary>
public sealed class QueryCachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly KeyedLock Locks = new();
    private readonly ICache _cache;
    private readonly ICacheKeyFactory _keyFactory;
    private readonly ICacheScope _cacheScope;
    private readonly ILogger<QueryCachingBehavior<TRequest, TResponse>> _logger;
    private readonly CachingOptions _cachingOptions;
    private readonly QueryCachingBehaviorOptions _behaviorOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryCachingBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    public QueryCachingBehavior(
        ICache cache,
        ICacheKeyFactory keyFactory,
        ICacheScope cacheScope,
        IOptions<CachingOptions> cachingOptions,
        IOptions<QueryCachingBehaviorOptions> behaviorOptions,
        ILogger<QueryCachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _keyFactory = keyFactory ?? throw new ArgumentNullException(nameof(keyFactory));
        _cacheScope = cacheScope ?? throw new ArgumentNullException(nameof(cacheScope));
        _cachingOptions = cachingOptions?.Value ?? throw new ArgumentNullException(nameof(cachingOptions));
        _behaviorOptions = behaviorOptions?.Value ?? throw new ArgumentNullException(nameof(behaviorOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_cachingOptions.Enabled || !_behaviorOptions.ShouldCache(request))
        {
            return await next().ConfigureAwait(false);
        }

        CacheKey key;
        try
        {
            key = BuildKey(request);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to build cache key for {Request}; bypassing cache.", typeof(TRequest).Name);
            return await next().ConfigureAwait(false);
        }

        var cached = await _cache.GetAsync<TResponse>(key, cancellationToken).ConfigureAwait(false);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit for {Request} ({Key})", typeof(TRequest).Name, key.FullKey);
            return cached.Value!;
        }

        var policy = _cachingOptions.StampedePolicy ?? CacheStampedePolicy.Default;
        if (policy.EnableLocking)
        {
            var gate = await Locks.TryAcquireAsync(key.FullKey, policy.LockTimeout, cancellationToken).ConfigureAwait(false);
            if (gate is null)
            {
                _logger.LogWarning("Failed to acquire cache lock for {Key} within {Timeout}ms; bypassing cache.", key.FullKey, policy.LockTimeout.TotalMilliseconds);
                return await next().ConfigureAwait(false);
            }

            try
            {
                cached = await _cache.GetAsync<TResponse>(key, cancellationToken).ConfigureAwait(false);
                if (cached is not null)
                {
                    _logger.LogDebug("Cache hit for {Request} ({Key})", typeof(TRequest).Name, key.FullKey);
                    return cached.Value!;
                }

                _logger.LogDebug("Cache miss for {Request} ({Key})", typeof(TRequest).Name, key.FullKey);
                var lockedResponse = await next().ConfigureAwait(false);
                if (!ShouldStore(request, lockedResponse))
                {
                    return lockedResponse;
                }

                var lockedEntryOptions = ResolveEntryOptions();
                await _cache.SetAsync(key, lockedResponse!, lockedEntryOptions, cancellationToken).ConfigureAwait(false);
                return lockedResponse;
            }
            finally
            {
                gate.Value.Dispose();
            }
        }

        _logger.LogDebug("Cache miss for {Request} ({Key})", typeof(TRequest).Name, key.FullKey);
        var response = await next().ConfigureAwait(false);

        if (!ShouldStore(request, response))
        {
            return response;
        }

        var entryOptions = ResolveEntryOptions();
        await _cache.SetAsync(key, response!, entryOptions, cancellationToken).ConfigureAwait(false);
        return response;
    }

    private CacheKey BuildKey(TRequest request)
    {
        var resource = _behaviorOptions.ResourceNameSelector?.Invoke(request) ?? typeof(TRequest).Name;
        var hash = request is ICacheKeyProvider provider
            ? provider.GetCacheHash(_keyFactory)
            : _behaviorOptions.HashFactory?.Invoke(request) ?? _keyFactory.CreateHash(request);
        return _cacheScope.Create(resource, hash);
    }

    private CacheEntryOptions ResolveEntryOptions()
    {
        var options = CloneEntryOptions(_cachingOptions.DefaultEntryOptions);

        if (_behaviorOptions.TtlByRequestType.TryGetValue(typeof(TRequest), out var ttl))
        {
            options.AbsoluteExpirationRelativeToNow = ttl;
        }
        else if (_behaviorOptions.DefaultTtl.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = _behaviorOptions.DefaultTtl;
        }

        return options;
    }

    private static CacheEntryOptions CloneEntryOptions(CacheEntryOptions? source)
    {
        if (source is null)
        {
            return new CacheEntryOptions();
        }

        return new CacheEntryOptions
        {
            AbsoluteExpiration = source.AbsoluteExpiration,
            AbsoluteExpirationRelativeToNow = source.AbsoluteExpirationRelativeToNow,
            SlidingExpiration = source.SlidingExpiration,
            Priority = source.Priority,
            Size = source.Size
        };
    }

    private bool ShouldStore(TRequest request, TResponse response) =>
        _behaviorOptions.ShouldCacheResponse(request, response);
}
