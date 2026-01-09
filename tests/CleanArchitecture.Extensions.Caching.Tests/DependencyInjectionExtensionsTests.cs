using System.Linq;
using CleanArchitecture.Extensions.Caching;
using CleanArchitecture.Extensions.Caching.Abstractions;
using CleanArchitecture.Extensions.Caching.Adapters;
using CleanArchitecture.Extensions.Caching.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Extensions.Caching.Tests;

public class DependencyInjectionExtensionsTests
{
    [Fact]
    public void Uses_memory_cache_adapter_by_default()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddCleanArchitectureCaching();

        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<ICache>();

        Assert.IsType<MemoryCacheAdapter>(cache);
    }

    [Fact]
    public void Uses_distributed_cache_when_non_memory_distributed_is_registered()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddCleanArchitectureCaching();
        services.AddSingleton<IDistributedCache, FakeDistributedCache>();

        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<ICache>();

        Assert.IsType<DistributedCacheAdapter>(cache);
    }

    [Fact]
    public void Throws_when_distributed_backend_selected_without_distributed_cache()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddCleanArchitectureCaching(options => options.Backend = CacheBackend.Distributed);
        // Remove the default distributed cache registration to simulate misconfiguration.
        var descriptor = services.First(d => d.ServiceType == typeof(IDistributedCache));
        services.Remove(descriptor);

        using var provider = services.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<ICache>());
    }

    private sealed class FakeDistributedCache : IDistributedCache
    {
        public byte[]? Get(string key) => null;
        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => Task.FromResult<byte[]?>(null);
        public void Refresh(string key) { }
        public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;
        public void Remove(string key) { }
        public Task RemoveAsync(string key, CancellationToken token = default) => Task.CompletedTask;
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) { }
        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) => Task.CompletedTask;
    }
}
