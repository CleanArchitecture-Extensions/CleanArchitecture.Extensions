using System.Text.Json;
using CleanArchitecture.Extensions.Caching;
using CleanArchitecture.Extensions.Caching.Abstractions;
using CleanArchitecture.Extensions.Caching.Keys;
using CleanArchitecture.Extensions.Caching.Options;
using CleanArchitecture.Extensions.Caching.Serialization;
using Microsoft.Extensions.DependencyInjection;

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

    [Fact]
    public void Uses_preferred_serializer_when_multiple_registered()
    {
        using var provider = BuildProvider(
            options => options.PreferredSerializer = "application/test+json",
            services => services.AddSingleton<ICacheSerializer, TestCacheSerializer>());

        var cache = provider.GetRequiredService<ICache>();
        var keyFactory = provider.GetRequiredService<ICacheKeyFactory>();
        var key = keyFactory.Create("Resource", keyFactory.CreateHash(new { id = 7 }));

        cache.Set(key, "value");
        var cached = cache.Get<string>(key);

        Assert.NotNull(cached);
        Assert.Equal("application/test+json", cached!.ContentType);
    }

    [Fact]
    public void Throws_when_preferred_serializer_is_missing()
    {
        using var provider = BuildProvider(options => options.PreferredSerializer = "missing/serializer");

        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<ICache>());
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
