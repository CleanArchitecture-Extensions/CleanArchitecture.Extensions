namespace CleanArchitecture.Extensions.Multitenancy.Tests;

public class TenantInfoTests
{
    [Fact]
    public void Constructor_trims_and_throws_on_empty()
    {
        Assert.Throws<ArgumentException>(() => new TenantInfo(" "));

        var tenant = new TenantInfo(" tenant ");

        Assert.Equal("tenant", tenant.TenantId);
    }

    [Fact]
    public void From_copies_properties_and_metadata()
    {
        var source = new TenantInfo("tenant-1")
        {
            InternalId = Guid.NewGuid(),
            Name = "Tenant One",
            Type = "paid",
            Region = "us-east",
            IsActive = false,
            IsSoftDeleted = true,
            CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            ExpiresAt = new DateTimeOffset(2024, 12, 31, 0, 0, 0, TimeSpan.Zero),
            ParentId = "parent",
            State = TenantState.Suspended
        };
        source.Metadata["Key"] = "Value";

        var copy = TenantInfo.From(source);

        Assert.Equal(source.TenantId, copy.TenantId);
        Assert.Equal(source.InternalId, copy.InternalId);
        Assert.Equal(source.Name, copy.Name);
        Assert.Equal(source.Type, copy.Type);
        Assert.Equal(source.Region, copy.Region);
        Assert.Equal(source.IsActive, copy.IsActive);
        Assert.Equal(source.IsSoftDeleted, copy.IsSoftDeleted);
        Assert.Equal(source.CreatedAt, copy.CreatedAt);
        Assert.Equal(source.ExpiresAt, copy.ExpiresAt);
        Assert.Equal(source.ParentId, copy.ParentId);
        Assert.Equal(source.State, copy.State);
        Assert.NotSame(source.Metadata, copy.Metadata);
        Assert.Equal("Value", copy.Metadata["key"]);
    }
}
