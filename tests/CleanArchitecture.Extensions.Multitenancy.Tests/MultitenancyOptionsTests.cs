using CleanArchitecture.Extensions.Multitenancy.Configuration;

namespace CleanArchitecture.Extensions.Multitenancy.Tests;

public class MultitenancyOptionsTests
{
    [Fact]
    public void Default_options_set_expected_defaults()
    {
        var options = MultitenancyOptions.Default;

        Assert.True(options.RequireTenantByDefault);
        Assert.False(options.AllowAnonymous);
        Assert.NotNull(options.HeaderNames);
        Assert.Contains("X-Tenant-ID", options.HeaderNames);
        Assert.Equal("tenant_id", options.ClaimType);
        Assert.Equal("tenantId", options.RouteParameterName);
        Assert.Equal("tenantId", options.QueryParameterName);
        Assert.Equal(TenantValidationMode.None, options.ValidationMode);
        Assert.True(options.AddTenantToLogScope);
        Assert.True(options.AddTenantToActivity);
        Assert.True(options.IncludeUnorderedProviders);
        Assert.NotNull(options.ResolutionOrder);
        Assert.Equal(TenantResolutionSource.Route, options.ResolutionOrder[0]);
        Assert.Equal(TenantResolutionSource.Default, options.ResolutionOrder[^1]);
    }
}
