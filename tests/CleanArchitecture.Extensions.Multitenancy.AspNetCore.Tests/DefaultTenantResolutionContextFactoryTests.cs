using System.Security.Claims;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Context;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Options;
using Microsoft.AspNetCore.Http;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Tests;

public class DefaultTenantResolutionContextFactoryTests
{
    [Fact]
    public void Create_populates_context_from_http_request()
    {
        var options = OptionsFactory.Create(new AspNetCoreMultitenancyOptions
        {
            CorrelationIdHeaderName = "X-Correlation-ID",
            UseTraceIdentifierAsCorrelationId = false
        });
        var factory = new DefaultTenantResolutionContextFactory(options);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Host = new HostString("tenant1.example.test");
        httpContext.Request.Headers["X-Correlation-ID"] = "corr-123";
        httpContext.Request.Headers["X-Tenant-ID"] = "tenant-1";
        httpContext.Request.RouteValues["tenantId"] = "tenant-1";
        httpContext.Request.QueryString = new QueryString("?tenantId=tenant-1");
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("tenant_id", "tenant-1")
        }, "test"));

        var context = factory.Create(httpContext);

        Assert.Equal("tenant1.example.test", context.Host);
        Assert.Equal("corr-123", context.CorrelationId);
        Assert.Equal("tenant-1", context.Headers["X-Tenant-ID"]);
        Assert.Equal("tenant-1", context.RouteValues["tenantId"]);
        Assert.Equal("tenant-1", context.Query["tenantId"]);
        Assert.Equal("tenant-1", context.Claims["tenant_id"]);
    }

    [Fact]
    public void Create_concatenates_multiple_claims_of_same_type()
    {
        var options = OptionsFactory.Create(new AspNetCoreMultitenancyOptions());
        var factory = new DefaultTenantResolutionContextFactory(options);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("tenant_id", "alpha"),
            new Claim("tenant_id", "beta")
        }, "test"));

        var context = factory.Create(httpContext);

        Assert.Equal("alpha;beta", context.Claims["tenant_id"]);
    }
}
