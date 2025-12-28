using CleanArchitecture.Extensions.Caching;
using CleanArchitecture.Extensions.Caching.Abstractions;
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Context;
using CleanArchitecture.Extensions.Multitenancy.Providers;
using CleanArchitecture.Extensions.Multitenancy.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Extensions.Multitenancy.Tests;

public class DependencyInjectionExtensionsTests
{
    [Fact]
    public void AddCleanArchitectureMultitenancy_registers_core_services()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCleanArchitectureMultitenancy();

        using var provider = services.BuildServiceProvider();

        var currentTenant = provider.GetRequiredService<ICurrentTenant>();
        var serializer = provider.GetRequiredService<ITenantContextSerializer>();
        var providers = provider.GetServices<ITenantProvider>().ToList();

        Assert.IsType<CurrentTenantAccessor>(currentTenant);
        Assert.IsType<SystemTextJsonTenantContextSerializer>(serializer);
        Assert.Contains(providers, descriptor => descriptor is RouteTenantProvider);
        Assert.Contains(providers, descriptor => descriptor is HostTenantProvider);
        Assert.Contains(providers, descriptor => descriptor is HeaderTenantProvider);
        Assert.Contains(providers, descriptor => descriptor is QueryTenantProvider);
        Assert.Contains(providers, descriptor => descriptor is ClaimTenantProvider);
        Assert.Contains(providers, descriptor => descriptor is DefaultTenantProvider);
    }

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
