using CleanArchitecture.Extensions.Caching.Abstractions;
using CleanArchitecture.Extensions.Caching.Behaviors;
using CleanArchitecture.Extensions.Caching.Keys;
using CleanArchitecture.Extensions.Caching.Options;
using CleanArchitecture.Extensions.Caching.Serialization;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MOptions = Microsoft.Extensions.Options.Options;

namespace CleanArchitecture.Extensions.Caching.Tests;

public class QueryCachingBehaviorTests
{
    private static QueryCachingBehavior<TRequest, TResponse> CreateBehavior<TRequest, TResponse>(
        ICache cache,
        QueryCachingBehaviorOptions? behaviorOptions = null,
        CachingOptions? cachingOptions = null)
        where TRequest : IRequest<TResponse>
    {
        var keyFactory = new DefaultCacheKeyFactory(MOptions.Create(cachingOptions ?? CachingOptions.Default));
        var cacheScope = new DefaultCacheScope(keyFactory, MOptions.Create(cachingOptions ?? CachingOptions.Default));
        var behavior = new QueryCachingBehavior<TRequest, TResponse>(
            cache,
            keyFactory,
            cacheScope,
            MOptions.Create(cachingOptions ?? CachingOptions.Default),
            MOptions.Create(behaviorOptions ?? new QueryCachingBehaviorOptions()),
            NullLogger<QueryCachingBehavior<TRequest, TResponse>>.Instance);
        return behavior;
    }

    private static ICache CreateCache(CachingOptions? options = null)
    {
        var serializer = new SystemTextJsonCacheSerializer();
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        return new Adapters.MemoryCacheAdapter(
            memoryCache,
            serializer,
            TimeProvider.System,
            MOptions.Create(options ?? CachingOptions.Default),
            NullLogger<Adapters.MemoryCacheAdapter>.Instance);
    }

    [Fact]
    public async Task Caches_successful_query_results()
    {
        var cache = CreateCache();
        var behavior = CreateBehavior<TestQuery, string>(cache, new QueryCachingBehaviorOptions { DefaultTtl = TimeSpan.FromMinutes(1) });
        var callCount = 0;
        var request = new TestQuery(1);
        RequestHandlerDelegate<string> next = _ =>
        {
            callCount++;
            return Task.FromResult("value");
        };

        var first = await behavior.Handle(request, next, CancellationToken.None);

        var second = await behavior.Handle(request, _ =>
        {
            callCount++;
            return Task.FromResult("other");
        }, CancellationToken.None);

        Assert.Equal("value", first);
        Assert.Equal("value", second);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task Respects_cache_predicate_and_bypasses_when_false()
    {
        var cache = CreateCache();
        var behaviorOptions = new QueryCachingBehaviorOptions
        {
            CachePredicate = _ => false
        };
        var behavior = CreateBehavior<TestQuery, string>(cache, behaviorOptions);
        var callCount = 0;
        var request = new TestQuery(2);
        RequestHandlerDelegate<string> next = _ =>
        {
            callCount++;
            return Task.FromResult("value");
        };

        await behavior.Handle(request, next, CancellationToken.None);

        await behavior.Handle(request, _ =>
        {
            callCount++;
            return Task.FromResult("other");
        }, CancellationToken.None);

        Assert.Equal(2, callCount);
    }

    [Fact]
    public async Task Respects_response_cache_predicate()
    {
        var cache = CreateCache();
        var behaviorOptions = new QueryCachingBehaviorOptions
        {
            ResponseCachePredicate = (_, response) => response is not "skip"
        };
        var behavior = CreateBehavior<TestQuery, string>(cache, behaviorOptions);
        var callCount = 0;
        var request = new TestQuery(3);
        RequestHandlerDelegate<string> failingNext = _ =>
        {
            callCount++;
            return Task.FromResult("skip");
        };

        await behavior.Handle(request, failingNext, CancellationToken.None);

        await behavior.Handle(request, _ =>
        {
            callCount++;
            return Task.FromResult("skip");
        }, CancellationToken.None);

        Assert.Equal(2, callCount);
    }

    [Fact]
    public async Task Propagates_cancellation_without_caching()
    {
        var cache = CreateCache();
        var behavior = CreateBehavior<TestQuery, string>(cache);
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var callCount = 0;
        RequestHandlerDelegate<string> next = _ =>
        {
            callCount++;
            return Task.FromResult("value");
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            behavior.Handle(new TestQuery(4), next, cts.Token));

        Assert.Equal(0, callCount);
    }

    [Fact]
    public async Task Default_predicate_skips_non_query_types()
    {
        var cache = CreateCache();
        var behavior = CreateBehavior<TestCommand, string>(cache);
        var callCount = 0;
        var request = new TestCommand(5);
        RequestHandlerDelegate<string> next = _ =>
        {
            callCount++;
            return Task.FromResult("value");
        };

        await behavior.Handle(request, next, CancellationToken.None);
        await behavior.Handle(request, _ =>
        {
            callCount++;
            return Task.FromResult("other");
        }, CancellationToken.None);

        Assert.Equal(2, callCount);
    }

    private sealed record TestQuery(int Id) : IRequest<string>;

    private sealed record TestCommand(int Id) : IRequest<string>;
}
