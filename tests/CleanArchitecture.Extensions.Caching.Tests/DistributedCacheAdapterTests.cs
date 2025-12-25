using CleanArchitecture.Extensions.Caching.Abstractions;
using CleanArchitecture.Extensions.Caching.Adapters;
using CleanArchitecture.Extensions.Caching.Keys;
using CleanArchitecture.Extensions.Caching.Options;
using CleanArchitecture.Extensions.Caching.Serialization;
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
            TimeProvider.System,
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
