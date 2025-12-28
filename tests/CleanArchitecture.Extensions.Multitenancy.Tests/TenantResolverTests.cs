using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using CleanArchitecture.Extensions.Multitenancy.Context;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.Tests;

public class TenantResolverTests
{
    [Fact]
    public async Task ResolveAsync_returns_null_when_not_resolved()
    {
        var options = Options.Create(new MultitenancyOptions());
        var strategy = new StubStrategy(TenantResolutionResult.NotFound(TenantResolutionSource.Header));
        var resolver = new TenantResolver(strategy, options, NullLogger<TenantResolver>.Instance);

        var result = await resolver.ResolveAsync(new TenantResolutionContext());

        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveAsync_uses_fallback_tenant_when_default_source()
    {
        var fallback = new TenantInfo("default") { Name = "Default Tenant" };
        var options = Options.Create(new MultitenancyOptions
        {
            FallbackTenant = fallback
        });

        var strategy = new StubStrategy(TenantResolutionResult.Resolved("default", TenantResolutionSource.Default));
        var resolver = new TenantResolver(strategy, options, NullLogger<TenantResolver>.Instance);

        var result = await resolver.ResolveAsync(new TenantResolutionContext());

        Assert.NotNull(result);
        Assert.Equal("default", result!.TenantId);
        Assert.True(result.IsValidated);
        Assert.Equal("Default Tenant", result.Tenant.Name);
    }

    [Fact]
    public async Task ResolveAsync_uses_fallback_tenant_id_when_default_source()
    {
        var options = Options.Create(new MultitenancyOptions
        {
            FallbackTenantId = "fallback-id"
        });

        var strategy = new StubStrategy(TenantResolutionResult.Resolved("fallback-id", TenantResolutionSource.Default));
        var resolver = new TenantResolver(strategy, options, NullLogger<TenantResolver>.Instance);

        var result = await resolver.ResolveAsync(new TenantResolutionContext());

        Assert.NotNull(result);
        Assert.Equal("fallback-id", result!.TenantId);
        Assert.True(result.IsValidated);
        Assert.True(result.Tenant.IsActive);
        Assert.Equal(TenantState.Active, result.Tenant.State);
    }

    [Fact]
    public async Task ResolveAsync_validates_using_cache()
    {
        var options = Options.Create(new MultitenancyOptions
        {
            ValidationMode = TenantValidationMode.Cache
        });
        var strategy = new StubStrategy(TenantResolutionResult.Resolved("tenant-1", TenantResolutionSource.Header));
        var cache = new StubTenantInfoCache(_ => new TenantInfo("tenant-1") { Name = "Cached" });
        var resolver = new TenantResolver(strategy, options, NullLogger<TenantResolver>.Instance, tenantCache: cache);

        var result = await resolver.ResolveAsync(new TenantResolutionContext());

        Assert.NotNull(result);
        Assert.True(result!.IsValidated);
        Assert.Equal("Cached", result.Tenant.Name);
        Assert.Equal(1, cache.GetCallCount);
    }

    [Fact]
    public async Task ResolveAsync_returns_inactive_tenant_when_cache_miss()
    {
        var options = Options.Create(new MultitenancyOptions
        {
            ValidationMode = TenantValidationMode.Cache
        });
        var strategy = new StubStrategy(TenantResolutionResult.Resolved("tenant-1", TenantResolutionSource.Header));
        var cache = new StubTenantInfoCache(_ => null);
        var resolver = new TenantResolver(strategy, options, NullLogger<TenantResolver>.Instance, tenantCache: cache);

        var result = await resolver.ResolveAsync(new TenantResolutionContext());

        Assert.NotNull(result);
        Assert.False(result!.IsValidated);
        Assert.False(result.Tenant.IsActive);
        Assert.Equal(TenantState.Unknown, result.Tenant.State);
    }

    [Fact]
    public async Task ResolveAsync_validates_using_repository_and_sets_cache()
    {
        var options = Options.Create(new MultitenancyOptions
        {
            ValidationMode = TenantValidationMode.Repository,
            ResolutionCacheTtl = TimeSpan.FromMinutes(2)
        });
        var strategy = new StubStrategy(TenantResolutionResult.Resolved("tenant-1", TenantResolutionSource.Header));
        var store = new StubTenantInfoStore(_ => new TenantInfo("tenant-1") { Name = "Stored" });
        var cache = new StubTenantInfoCache(_ => null);
        var resolver = new TenantResolver(strategy, options, NullLogger<TenantResolver>.Instance, store, cache);

        var result = await resolver.ResolveAsync(new TenantResolutionContext());

        Assert.NotNull(result);
        Assert.True(result!.IsValidated);
        Assert.Equal("Stored", result.Tenant.Name);
        Assert.Equal(1, store.CallCount);
        Assert.Equal(1, cache.SetCallCount);
        Assert.Equal(TimeSpan.FromMinutes(2), cache.LastTtl);
    }

    private sealed class StubStrategy : ITenantResolutionStrategy
    {
        private readonly TenantResolutionResult _result;

        public StubStrategy(TenantResolutionResult result)
        {
            _result = result;
        }

        public Task<TenantResolutionResult> ResolveAsync(TenantResolutionContext context, CancellationToken cancellationToken = default) =>
            Task.FromResult(_result);
    }
}
