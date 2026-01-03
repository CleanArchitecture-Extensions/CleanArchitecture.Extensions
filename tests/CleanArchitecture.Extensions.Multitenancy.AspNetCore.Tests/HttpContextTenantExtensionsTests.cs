using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Context;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Tests;

public class HttpContextTenantExtensionsTests
{
    [Fact]
    public void GetTenantContext_uses_configured_item_key()
    {
        var services = new ServiceCollection();
        services.Configure<AspNetCoreMultitenancyOptions>(options => options.HttpContextItemKey = "custom-key");
        using var provider = services.BuildServiceProvider();

        var tenantContext = new TenantContext(
            new TenantInfo("tenant-1"),
            TenantResolutionResult.Resolved("tenant-1", TenantResolutionSource.Header));

        var httpContext = new DefaultHttpContext
        {
            RequestServices = provider
        };
        httpContext.Items["custom-key"] = tenantContext;

        var resolved = httpContext.GetTenantContext();

        Assert.Same(tenantContext, resolved);
    }

    [Fact]
    public void GetTenantContext_falls_back_to_default_key()
    {
        var services = new ServiceCollection();
        using var provider = services.BuildServiceProvider();

        var tenantContext = new TenantContext(
            new TenantInfo("tenant-1"),
            TenantResolutionResult.Resolved("tenant-1", TenantResolutionSource.Header));

        var httpContext = new DefaultHttpContext
        {
            RequestServices = provider
        };
        httpContext.Items[AspNetCoreMultitenancyDefaults.TenantContextItemKey] = tenantContext;

        var resolved = httpContext.GetTenantContext();

        Assert.Same(tenantContext, resolved);
    }
}
