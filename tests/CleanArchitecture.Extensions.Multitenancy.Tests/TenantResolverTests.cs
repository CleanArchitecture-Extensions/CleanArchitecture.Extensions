using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using CleanArchitecture.Extensions.Multitenancy.Context;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.Tests;

public class TenantResolverTests
{
    [Fact]
    public async Task ResolveAsync_uses_fallback_tenant_for_default_source()
    {
        var options = Options.Create(new MultitenancyOptions
        {
            FallbackTenantId = "fallback"
        });

        var strategy = new StubResolutionStrategy(
            TenantResolutionResult.Resolved("ignored", TenantResolutionSource.Default));

        var resolver = new TenantResolver(
            strategy,
            options,
            NullLogger<TenantResolver>.Instance);

        var context = await resolver.ResolveAsync(new TenantResolutionContext());

        Assert.NotNull(context);
        Assert.Equal("fallback", context!.TenantId);
        Assert.True(context.IsValidated);
        Assert.Equal(TenantResolutionSource.Default, context.Source);
    }

    [Fact]
    public async Task ResolveAsync_returns_unvalidated_context_when_repository_missing()
    {
        var options = Options.Create(new MultitenancyOptions
        {
            ValidationMode = TenantValidationMode.Repository
        });

        var strategy = new StubResolutionStrategy(
            TenantResolutionResult.Resolved("tenant-1", TenantResolutionSource.Header));

        var resolver = new TenantResolver(
            strategy,
            options,
            NullLogger<TenantResolver>.Instance,
            tenantStore: new NullTenantStore());

        var context = await resolver.ResolveAsync(new TenantResolutionContext());

        Assert.NotNull(context);
        Assert.False(context!.IsValidated);
        Assert.Equal(TenantState.Unknown, context.Tenant?.State);
        Assert.False(context.Tenant?.IsActive);
    }

    private sealed class StubResolutionStrategy : ITenantResolutionStrategy
    {
        private readonly TenantResolutionResult _result;

        public StubResolutionStrategy(TenantResolutionResult result)
        {
            _result = result;
        }

        public Task<TenantResolutionResult> ResolveAsync(
            TenantResolutionContext context,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_result);
    }

    private sealed class NullTenantStore : ITenantInfoStore
    {
        public Task<ITenantInfo?> FindByIdAsync(string tenantId, CancellationToken cancellationToken = default)
            => Task.FromResult<ITenantInfo?>(null);
    }
}
