using CleanArchitecture.Extensions.Multitenancy.Context;

namespace CleanArchitecture.Extensions.Multitenancy.Tests;

public class CurrentTenantAccessorTests
{
    [Fact]
    public void BeginScope_restores_prior_context()
    {
        var accessor = new CurrentTenantAccessor();

        Assert.Null(accessor.Current);

        var tenant = new TenantInfo("tenant-1");
        var context = new TenantContext(tenant, TenantResolutionResult.Resolved("tenant-1", TenantResolutionSource.Custom));

        using (accessor.BeginScope(context))
        {
            Assert.Equal("tenant-1", accessor.TenantId);
        }

        Assert.Null(accessor.Current);
    }

    [Fact]
    public void Properties_reflect_current_context()
    {
        var accessor = new CurrentTenantAccessor();
        var tenant = new TenantInfo("tenant-1");
        var resolution = TenantResolutionResult.Resolved("tenant-1", TenantResolutionSource.Header, TenantResolutionConfidence.High);
        accessor.Current = new TenantContext(tenant, resolution, isValidated: true);

        Assert.True(accessor.IsResolved);
        Assert.True(accessor.IsValidated);
        Assert.Equal(TenantResolutionSource.Header, accessor.Source);
        Assert.Equal(TenantResolutionConfidence.High, accessor.Confidence);
    }
}
