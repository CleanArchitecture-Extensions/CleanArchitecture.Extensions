using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using CleanArchitecture.Extensions.Multitenancy.Context;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.Tests;

public class TenantResolverTests
{
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
