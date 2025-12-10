using CleanArchitecture.Extensions.Caching;
using CleanArchitecture.Extensions.Caching.Abstractions;
using CleanArchitecture.Extensions.Caching.Adapters;
using CleanArchitecture.Extensions.Caching.Keys;
using CleanArchitecture.Extensions.Caching.Options;
using CleanArchitecture.Extensions.Caching.Serialization;
using CleanArchitecture.Extensions.Core.Results;
using CleanArchitecture.Extensions.Core.Time;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Caching.Tests;

public class MemoryCacheAdapterTests
{
    private static ServiceProvider BuildProvider(Action<CachingOptions>? configure = null, Action<IServiceCollection>? configureServices = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCleanArchitectureCaching(options =>
        {
            options.DefaultNamespace = "Tests";
            configure?.Invoke(options);
        });
        services.AddSingleton<IClock>(_ => new FrozenClock(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)));
        configureServices?.Invoke(services);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void Get_and_set_roundtrip()
    {
        using var provider = BuildProvider();
        var cache = provider.GetRequiredService<ICache>();
        var keyFactory = provider.GetRequiredService<ICacheKeyFactory>();
        var key = keyFactory.Create("Resource", keyFactory.CreateHash(new { id = 1 }));

        cache.Set(key, "value", new CacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) });
        var cached = cache.Get<string>(key);

        Assert.NotNull(cached);
        Assert.Equal("value", cached!.Value);
        Assert.Equal("application/json", cached.ContentType);
    }

    [Fact]
    public void GetOrAdd_caches_value_and_reuses_on_next_call()
    {
        using var provider = BuildProvider();
        var cache = provider.GetRequiredService<ICache>();
        var keyFactory = provider.GetRequiredService<ICacheKeyFactory>();
        var key = keyFactory.Create("Resource", keyFactory.CreateHash(new { id = 2 }));
        var callCount = 0;

        var first = cache.GetOrAdd(key, () =>
        {
            callCount++;
            return "computed";
        });

        var second = cache.GetOrAdd(key, () =>
        {
            callCount++;
            return "other";
        });

        Assert.Equal("computed", first);
        Assert.Equal("computed", second);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task GetOrAddResult_caches_only_successful_results()
    {
        using var provider = BuildProvider();
        var cache = provider.GetRequiredService<ICache>();
        var keyFactory = provider.GetRequiredService<ICacheKeyFactory>();
        var successKey = keyFactory.Create("Resource", keyFactory.CreateHash(new { id = 3 }));
        var failureKey = keyFactory.Create("Resource", keyFactory.CreateHash(new { id = 4 }));

        var successResult = cache.GetOrAddResult<string>(successKey, () => Result.Success<string>("ok"));
        var failureResult = cache.GetOrAddResult<string>(failureKey, () => Result.Failure<string>(new Error("error", "error")));

        var successCached = cache.GetOrAddResult<string>(successKey, () => throw new InvalidOperationException("should not run"));
        var failureCached = cache.GetOrAddResult<string>(failureKey, () => Result.Failure<string>(new Error("another", "another")));

        Assert.True(successResult.IsSuccess);
        Assert.True(successCached.IsSuccess);
        Assert.Equal("ok", successCached.Value);

        Assert.True(failureResult.IsFailure);
        Assert.True(failureCached.IsFailure);
        Assert.Equal("another", failureCached.Errors.Single().Code);
    }

    [Fact]
    public void Remove_evicts_item()
    {
        using var provider = BuildProvider();
        var cache = provider.GetRequiredService<ICache>();
        var keyFactory = provider.GetRequiredService<ICacheKeyFactory>();
        var key = keyFactory.Create("Resource", keyFactory.CreateHash(new { id = 5 }));

        cache.Set(key, "value");
        cache.Remove(key);

        Assert.Null(cache.Get<string>(key));
    }

    [Fact]
    public void Skips_store_when_size_exceeds_limit()
    {
        using var provider = BuildProvider(options => options.MaxEntrySizeBytes = 4);
        var cache = provider.GetRequiredService<ICache>();
        var keyFactory = provider.GetRequiredService<ICacheKeyFactory>();
        var key = keyFactory.Create("Resource", keyFactory.CreateHash(new { id = 6 }));

        cache.Set(key, "toolarge");

        Assert.Null(cache.Get<string>(key));
    }
}
