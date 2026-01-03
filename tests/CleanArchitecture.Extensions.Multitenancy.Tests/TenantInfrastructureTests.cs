using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Serialization;

namespace CleanArchitecture.Extensions.Multitenancy.Tests;

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
