using CleanArchitecture.Extensions.Caching.Abstractions;
using CleanArchitecture.Extensions.Caching.Keys;
using CleanArchitecture.Extensions.Caching.Options;
using CleanArchitecture.Extensions.Core.Results;
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

        var key = BuildKey(request);
        var cached = await _cache.GetAsync<TResponse>(key, cancellationToken).ConfigureAwait(false);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit for {Request} ({Key})", typeof(TRequest).Name, key.FullKey);
            return cached.Value!;
        }

        _logger.LogDebug("Cache miss for {Request} ({Key})", typeof(TRequest).Name, key.FullKey);
        var response = await next().ConfigureAwait(false);

        if (!ShouldStore(response))
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
        var hash = _behaviorOptions.HashFactory?.Invoke(request) ?? _keyFactory.CreateHash(request);
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

    private bool ShouldStore(TResponse response)
    {
        if (!_behaviorOptions.CacheNullValues && response is null)
        {
            return false;
        }

        if (_behaviorOptions.BypassOnError && response is Result result && result.IsFailure)
        {
            return false;
        }

        return true;
    }
}
