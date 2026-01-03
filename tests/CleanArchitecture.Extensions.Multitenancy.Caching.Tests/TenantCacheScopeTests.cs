using CleanArchitecture.Extensions.Caching.Keys;
using CleanArchitecture.Extensions.Caching.Options;
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Context;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.Caching.Tests;

public class TenantCacheScopeTests
{
    [Fact]
    public void Create_includes_tenant_and_namespace()
    {
        var currentTenant = new CurrentTenantAccessor
        {
            Current = new TenantContext(new TenantInfo("tenant-1"), TenantResolutionResult.Resolved("tenant-1", TenantResolutionSource.Header))
        };
        var options = Options.Create(new CachingOptions { DefaultNamespace = "Tests" });
        var keyFactory = new DefaultCacheKeyFactory(options);
        var scope = new TenantCacheScope(currentTenant, keyFactory, options);

        var key = scope.Create("Resource", "hash");

        Assert.Equal("Tests:tenant-1:Resource:hash", key.FullKey);
    }

    [Fact]
    public void CreateForRequest_uses_request_type_name()
    {
        var currentTenant = new CurrentTenantAccessor
        {
            Current = new TenantContext(new TenantInfo("tenant-1"), TenantResolutionResult.Resolved("tenant-1", TenantResolutionSource.Header))
        };
        var options = Options.Create(new CachingOptions { DefaultNamespace = "Tests" });
        var keyFactory = new DefaultCacheKeyFactory(options);
        var scope = new TenantCacheScope(currentTenant, keyFactory, options);

        var key = scope.CreateForRequest<TestRequest>("hash");

        Assert.Equal("Tests:tenant-1:TestRequest:hash", key.FullKey);
    }

    private sealed record TestRequest;
}
