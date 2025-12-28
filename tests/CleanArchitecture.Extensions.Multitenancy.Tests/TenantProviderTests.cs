using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using CleanArchitecture.Extensions.Multitenancy.Providers;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.Tests;

public class HeaderTenantProviderTests
{
    [Fact]
    public async Task ResolveAsync_splits_header_values()
    {
        var options = Options.Create(new MultitenancyOptions
        {
            HeaderNames = new[] { "X-Tenant-ID" }
        });
        var context = new TenantResolutionContext();
        context.Headers["X-Tenant-ID"] = "tenant-a, tenant-b";

        var provider = new HeaderTenantProvider(options);

        var result = await provider.ResolveAsync(context);

        Assert.False(result.IsResolved);
        Assert.True(result.IsAmbiguous);
        Assert.Equal(TenantResolutionSource.Header, result.Source);
        Assert.Equal(2, result.Candidates.Count);
    }
}

public class QueryTenantProviderTests
{
    [Fact]
    public async Task ResolveAsync_returns_not_found_when_query_name_missing()
    {
        var options = Options.Create(new MultitenancyOptions
        {
            QueryParameterName = string.Empty
        });

        var provider = new QueryTenantProvider(options);

        var result = await provider.ResolveAsync(new TenantResolutionContext());

        Assert.False(result.IsResolved);
        Assert.Equal(TenantResolutionSource.QueryString, result.Source);
    }
}

public class RouteTenantProviderTests
{
    [Fact]
    public async Task ResolveAsync_returns_high_confidence_for_route_value()
    {
        var options = Options.Create(new MultitenancyOptions
        {
            RouteParameterName = "tenantId"
        });
        var context = new TenantResolutionContext();
        context.RouteValues["tenantId"] = "tenant-a";

        var provider = new RouteTenantProvider(options);

        var result = await provider.ResolveAsync(context);

        Assert.True(result.IsResolved);
        Assert.Equal("tenant-a", result.TenantId);
        Assert.Equal(TenantResolutionConfidence.High, result.Confidence);
    }
}

public class ClaimTenantProviderTests
{
    [Fact]
    public async Task ResolveAsync_returns_not_found_when_claim_type_missing()
    {
        var options = Options.Create(new MultitenancyOptions
        {
            ClaimType = string.Empty
        });

        var provider = new ClaimTenantProvider(options);

        var result = await provider.ResolveAsync(new TenantResolutionContext());

        Assert.False(result.IsResolved);
        Assert.Equal(TenantResolutionSource.Claim, result.Source);
    }
}

public class HostTenantProviderTests
{
    [Theory]
    [InlineData("tenant.example.com", "tenant")]
    [InlineData("tenant.example.com:5000", "tenant")]
    [InlineData("localhost", null)]
    [InlineData("127.0.0.1", null)]
    [InlineData("[::1]:5000", null)]
    public async Task ResolveAsync_uses_default_host_selector(string host, string? expectedTenant)
    {
        var options = Options.Create(new MultitenancyOptions());
        var context = new TenantResolutionContext { Host = host };
        var provider = new HostTenantProvider(options);

        var result = await provider.ResolveAsync(context);

        Assert.Equal(expectedTenant is not null, result.IsResolved);
        Assert.Equal(expectedTenant, result.TenantId);
    }
}

public class DefaultTenantProviderTests
{
    [Fact]
    public async Task ResolveAsync_uses_fallback_tenant()
    {
        var options = Options.Create(new MultitenancyOptions
        {
            FallbackTenant = new TenantInfo("tenant-a")
        });

        var provider = new DefaultTenantProvider(options);

        var result = await provider.ResolveAsync(new TenantResolutionContext());

        Assert.True(result.IsResolved);
        Assert.Equal("tenant-a", result.TenantId);
        Assert.Equal(TenantResolutionSource.Default, result.Source);
    }

    [Fact]
    public async Task ResolveAsync_uses_fallback_tenant_id()
    {
        var options = Options.Create(new MultitenancyOptions
        {
            FallbackTenantId = "tenant-b"
        });

        var provider = new DefaultTenantProvider(options);

        var result = await provider.ResolveAsync(new TenantResolutionContext());

        Assert.True(result.IsResolved);
        Assert.Equal("tenant-b", result.TenantId);
    }
}

public class DelegateTenantProviderTests
{
    [Fact]
    public async Task ResolveAsync_returns_candidates_from_delegate()
    {
        var provider = new DelegateTenantProvider(
            _ => new[] { "tenant-a", "tenant-b" },
            TenantResolutionSource.Custom,
            TenantResolutionConfidence.Low);

        var result = await provider.ResolveAsync(new TenantResolutionContext());

        Assert.False(result.IsResolved);
        Assert.True(result.IsAmbiguous);
        Assert.Equal(TenantResolutionSource.Custom, result.Source);
    }

    [Fact]
    public async Task ResolveAsync_ignores_empty_values_from_string_delegate()
    {
        var provider = new DelegateTenantProvider(_ => " ", TenantResolutionSource.Custom);

        var result = await provider.ResolveAsync(new TenantResolutionContext());

        Assert.False(result.IsResolved);
        Assert.Empty(result.Candidates);
    }
}
