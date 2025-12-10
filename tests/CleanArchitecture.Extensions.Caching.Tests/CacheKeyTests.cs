using CleanArchitecture.Extensions.Caching.Keys;

namespace CleanArchitecture.Extensions.Caching.Tests;

public class CacheKeyTests
{
    [Fact]
    public void Formats_full_key_without_tenant()
    {
        var key = new CacheKey("ns", "ResourceQuery", "abc123");

        Assert.Equal("ns:ResourceQuery:abc123", key.FullKey);
        Assert.Equal(key.FullKey, key.ToString());
    }

    [Fact]
    public void Formats_full_key_with_tenant()
    {
        var key = new CacheKey("ns", "ResourceQuery", "abc123", "tenant-1");

        Assert.Equal("ns:tenant-1:ResourceQuery:abc123", key.FullKey);
    }

    [Fact]
    public void Trims_segments_and_normalizes_empty_tenant_to_null()
    {
        var key = new CacheKey(" ns ", " Resource ", " hash ", "   ");

        Assert.Equal("ns", key.Namespace);
        Assert.Equal("Resource", key.Resource);
        Assert.Equal("hash", key.Hash);
        Assert.Null(key.TenantId);
    }

    [Theory]
    [InlineData(null, "resource", "hash")]
    [InlineData("", "resource", "hash")]
    [InlineData(" ", "resource", "hash")]
    [InlineData("ns", null, "hash")]
    [InlineData("ns", " ", "hash")]
    [InlineData("ns", "resource", null)]
    [InlineData("ns", "resource", " ")]
    public void Throws_when_required_segments_missing(string? ns, string? resource, string? hash)
    {
        Assert.Throws<ArgumentException>(() => new CacheKey(ns!, resource!, hash!));
    }
}
