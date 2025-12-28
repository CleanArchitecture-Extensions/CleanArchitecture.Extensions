using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using CleanArchitecture.Extensions.Multitenancy.Providers;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.Tests;

public class CompositeTenantResolutionStrategyTests
{
    [Fact]
    public async Task Resolution_uses_configured_order_over_registration_order()
    {
        var options = Options.Create(new MultitenancyOptions());
        var providers = new ITenantProvider[]
        {
            new DelegateTenantProvider(_ => "header-tenant", TenantResolutionSource.Header),
            new DelegateTenantProvider(_ => "route-tenant", TenantResolutionSource.Route)
        };

        var strategy = new CompositeTenantResolutionStrategy(providers, options);

        var result = await strategy.ResolveAsync(new TenantResolutionContext());

        Assert.True(result.IsResolved);
        Assert.Equal("route-tenant", result.TenantId);
        Assert.Equal(TenantResolutionSource.Route, result.Source);
    }

    [Fact]
    public async Task Resolution_requires_consensus_when_enabled()
    {
        var options = Options.Create(new MultitenancyOptions
        {
            RequireMatchAcrossSources = true
        });

        var providers = new ITenantProvider[]
        {
            new DelegateTenantProvider(_ => "tenant-a", TenantResolutionSource.Header),
            new DelegateTenantProvider(_ => "tenant-b", TenantResolutionSource.Route)
        };

        var strategy = new CompositeTenantResolutionStrategy(providers, options);

        var result = await strategy.ResolveAsync(new TenantResolutionContext());

        Assert.False(result.IsResolved);
        Assert.True(result.IsAmbiguous);
        Assert.Equal(TenantResolutionSource.Composite, result.Source);
    }

    [Fact]
    public async Task Resolution_returns_single_candidate_when_consensus_matches()
    {
        var options = Options.Create(new MultitenancyOptions
        {
            RequireMatchAcrossSources = true
        });

        var providers = new ITenantProvider[]
        {
            new DelegateTenantProvider(_ => "tenant-a", TenantResolutionSource.Header),
            new DelegateTenantProvider(_ => "tenant-a", TenantResolutionSource.Route)
        };

        var strategy = new CompositeTenantResolutionStrategy(providers, options);

        var result = await strategy.ResolveAsync(new TenantResolutionContext());

        Assert.True(result.IsResolved);
        Assert.Equal("tenant-a", result.TenantId);
        Assert.Equal(TenantResolutionSource.Composite, result.Source);
    }

    [Fact]
    public async Task Resolution_includes_unordered_providers_when_enabled()
    {
        var options = Options.Create(new MultitenancyOptions
        {
            IncludeUnorderedProviders = true
        });

        var providers = new ITenantProvider[]
        {
            new DelegateTenantProvider(_ => "custom-tenant", TenantResolutionSource.Custom)
        };

        var strategy = new CompositeTenantResolutionStrategy(providers, options);

        var result = await strategy.ResolveAsync(new TenantResolutionContext());

        Assert.True(result.IsResolved);
        Assert.Equal("custom-tenant", result.TenantId);
    }

    [Fact]
    public async Task Resolution_skips_unordered_providers_when_disabled()
    {
        var options = Options.Create(new MultitenancyOptions
        {
            IncludeUnorderedProviders = false
        });

        var providers = new ITenantProvider[]
        {
            new DelegateTenantProvider(_ => "custom-tenant", TenantResolutionSource.Custom)
        };

        var strategy = new CompositeTenantResolutionStrategy(providers, options);

        var result = await strategy.ResolveAsync(new TenantResolutionContext());

        Assert.False(result.IsResolved);
        Assert.Equal(TenantResolutionSource.Composite, result.Source);
    }
}
