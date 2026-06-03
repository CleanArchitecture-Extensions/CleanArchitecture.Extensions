using System.Text.Json;
using CleanArchitecture.Extensions.Caching.Abstractions;
using CleanArchitecture.Extensions.Caching.Adapters;
using CleanArchitecture.Extensions.Caching.Keys;
using CleanArchitecture.Extensions.Caching.Options;
using CleanArchitecture.Extensions.Caching.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using MOptions = Microsoft.Extensions.Options.Options;

namespace CleanArchitecture.Extensions.Caching.Tests;

public class DistributedCacheAdapterTests
{
    private static DistributedCacheAdapter CreateAdapter(CachingOptions? options = null, ICacheSerializer? serializer = null, IDistributedCache? distributed = null)
    {
        serializer ??= new SystemTextJsonCacheSerializer();
        distributed ??= new MemoryDistributedCache(MOptions.Create(new MemoryDistributedCacheOptions()));
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

    [Fact]
    public void Get_removes_corrupt_payload()
    {
        var distributed = new MemoryDistributedCache(MOptions.Create(new MemoryDistributedCacheOptions()));
        var adapter = CreateAdapter(distributed: distributed);
        var key = new CacheKey("ns", "Resource", "corrupt-sync");
        distributed.Set(key.FullKey, "not-json"u8.ToArray(), new DistributedCacheEntryOptions());

        var cached = adapter.Get<string>(key);

        Assert.Null(cached);
        Assert.Null(distributed.Get(key.FullKey));
    }

    [Fact]
    public async Task GetAsync_removes_corrupt_payload()
    {
        var distributed = new MemoryDistributedCache(MOptions.Create(new MemoryDistributedCacheOptions()));
        var adapter = CreateAdapter(distributed: distributed);
        var key = new CacheKey("ns", "Resource", "corrupt-async");
        await distributed.SetAsync(key.FullKey, "not-json"u8.ToArray(), new DistributedCacheEntryOptions());

        var cached = await adapter.GetAsync<string>(key);

        Assert.Null(cached);
        Assert.Null(await distributed.GetAsync(key.FullKey));
    }

    [Fact]
    public async Task Uses_serializer_content_type()
    {
        var adapter = CreateAdapter(serializer: new TestCacheSerializer());
        var key = new CacheKey("ns", "Resource", "content");

        await adapter.SetAsync(key, "value");
        var cached = await adapter.GetAsync<string>(key);

        Assert.NotNull(cached);
        Assert.Equal("application/test+json", cached!.ContentType);
    }

    private sealed class TestCacheSerializer : ICacheSerializer
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = false
        };

        public string ContentType => "application/test+json";

        public byte[] Serialize<T>(T? value) => JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions);

        public T? Deserialize<T>(ReadOnlySpan<byte> payload) => JsonSerializer.Deserialize<T>(payload, SerializerOptions);
    }
}
