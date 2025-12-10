using CleanArchitecture.Extensions.Caching.Abstractions;
using CleanArchitecture.Extensions.Caching.Adapters;
using CleanArchitecture.Extensions.Caching.Keys;
using CleanArchitecture.Extensions.Caching.Options;
using CleanArchitecture.Extensions.Caching.Serialization;
using CleanArchitecture.Extensions.Core.Results;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MOptions = Microsoft.Extensions.Options.Options;

namespace CleanArchitecture.Extensions.Caching.Tests;

public class DistributedCacheAdapterTests
{
    private static DistributedCacheAdapter CreateAdapter(CachingOptions? options = null)
    {
        var serializer = new SystemTextJsonCacheSerializer();
        var distributed = new MemoryDistributedCache(MOptions.Create(new MemoryDistributedCacheOptions()));
        var cachingOptions = options ?? CachingOptions.Default;
        return new DistributedCacheAdapter(
            distributed,
            serializer,
            MOptions.Create(cachingOptions),
            NullLogger<DistributedCacheAdapter>.Instance);
    }

    [Fact]
    public async Task Set_and_get_roundtrip()
    {
        var adapter = CreateAdapter();
        var key = new CacheKey("ns", "Resource", "hash");

        await adapter.SetAsync(key, "value", new CacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) });
        var cached = await adapter.GetAsync<string>(key);

        Assert.NotNull(cached);
        Assert.Equal("value", cached!.Value);
    }

    [Fact]
    public void GetOrAdd_uses_cached_value_after_first_call()
    {
        var adapter = CreateAdapter();
        var key = new CacheKey("ns", "Resource", "hash");
        var callCount = 0;

        var first = adapter.GetOrAdd(key, () =>
        {
            callCount++;
            return "computed";
        });

        var second = adapter.GetOrAdd(key, () =>
        {
            callCount++;
            return "other";
        });

        Assert.Equal("computed", first);
        Assert.Equal("computed", second);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void GetOrAddResult_caches_success_only()
    {
        var adapter = CreateAdapter();
        var successKey = new CacheKey("ns", "Resource", "success");
        var failureKey = new CacheKey("ns", "Resource", "fail");
        var successCalled = 0;
        var failureCalled = 0;

        var success = adapter.GetOrAddResult<string>(successKey, () =>
        {
            successCalled++;
            return Result.Success<string>("ok");
        });
        var failure = adapter.GetOrAddResult<string>(failureKey, () =>
        {
            failureCalled++;
            return Result.Failure<string>(new Error("err", "failed"));
        });

        var successCached = adapter.GetOrAddResult<string>(successKey, () =>
        {
            successCalled++;
            return Result.Success<string>("should-not-run");
        });
        var failureCached = adapter.GetOrAddResult<string>(failureKey, () =>
        {
            failureCalled++;
            return Result.Failure<string>(new Error("err2", "failed again"));
        });

        Assert.True(success.IsSuccess);
        Assert.Equal("ok", successCached.Value);
        Assert.True(failure.IsFailure);
        Assert.True(failureCached.IsFailure);
        Assert.Equal("err2", failureCached.Errors.Single().Code);
        Assert.Equal(1, successCalled);
        Assert.Equal(2, failureCalled);
    }

    [Fact]
    public void Remove_clears_entry()
    {
        var adapter = CreateAdapter();
        var key = new CacheKey("ns", "Resource", "remove");

        adapter.Set(key, "value");
        adapter.Remove(key);

        Assert.Null(adapter.Get<string>(key));
    }

    [Fact]
    public void Skips_storing_when_payload_exceeds_limit()
    {
        var adapter = CreateAdapter(new CachingOptions { MaxEntrySizeBytes = 4 });
        var key = new CacheKey("ns", "Resource", "size");

        adapter.Set(key, "toolarge");

        Assert.Null(adapter.Get<string>(key));
    }
}
