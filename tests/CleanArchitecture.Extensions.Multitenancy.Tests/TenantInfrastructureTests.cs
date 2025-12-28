using CleanArchitecture.Extensions.Caching.Keys;
using CleanArchitecture.Extensions.Caching.Options;
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Context;
using CleanArchitecture.Extensions.Multitenancy.Serialization;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.Tests;

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

public class SystemTextJsonTenantContextSerializerTests
{
    [Fact]
    public void Serialize_round_trips_context()
    {
        var tenant = new TenantInfo("tenant-1")
        {
            Name = "Tenant One",
            Region = "us-east"
        };
        tenant.Metadata["color"] = "blue";

        var resolution = TenantResolutionResult.Resolved("tenant-1", TenantResolutionSource.Header, TenantResolutionConfidence.High);
        var context = new TenantContext(tenant, resolution, "corr-1", isValidated: false)
        {
            ResolvedAt = new DateTimeOffset(2024, 2, 1, 12, 0, 0, TimeSpan.Zero)
        };

        var serializer = new SystemTextJsonTenantContextSerializer();

        var payload = serializer.Serialize(context);
        var roundTrip = serializer.Deserialize(payload);

        Assert.Equal("tenant-1", roundTrip.TenantId);
        Assert.Equal("corr-1", roundTrip.CorrelationId);
        Assert.False(roundTrip.IsValidated);
        Assert.Equal(context.ResolvedAt, roundTrip.ResolvedAt);
        Assert.Equal(TenantResolutionSource.Header, roundTrip.Source);
        Assert.Equal(TenantResolutionConfidence.High, roundTrip.Confidence);
        Assert.Equal("blue", roundTrip.Tenant.Metadata["color"]);
    }

    [Fact]
    public void Serialize_throws_on_null_context()
    {
        var serializer = new SystemTextJsonTenantContextSerializer();

        Assert.Throws<ArgumentNullException>(() => serializer.Serialize(null!));
    }

    [Fact]
    public void Deserialize_throws_on_empty_payload()
    {
        var serializer = new SystemTextJsonTenantContextSerializer();

        Assert.Throws<ArgumentException>(() => serializer.Deserialize(" "));
    }
}
