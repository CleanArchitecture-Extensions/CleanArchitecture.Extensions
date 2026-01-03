using CleanArchitecture.Extensions.Caching;
using CleanArchitecture.Extensions.Caching.Abstractions;
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Context;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Extensions.Multitenancy.Caching.Tests;

public class DependencyInjectionExtensionsTests
{
    [Fact]
    public void AddCleanArchitectureMultitenancyCaching_throws_when_caching_missing()
    {
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.AddCleanArchitectureMultitenancyCaching());
    }

    [Fact]
    public void AddCleanArchitectureMultitenancyCaching_replaces_cache_scope()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCleanArchitectureCaching();
        services.AddCleanArchitectureMultitenancy();
        services.AddCleanArchitectureMultitenancyCaching();

        using var provider = services.BuildServiceProvider();

        var scope = provider.GetRequiredService<ICacheScope>();

        Assert.IsType<TenantCacheScope>(scope);
    }
}
