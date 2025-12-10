using CleanArchitecture.Extensions.Caching.Abstractions;
using CleanArchitecture.Extensions.Caching.Keys;
using CleanArchitecture.Extensions.Caching.Options;

namespace CleanArchitecture.Extensions.Caching.Tests;

public class CacheItemTests
{
    [Fact]
    public void Captures_metadata_and_value()
    {
        var key = new CacheKey("ns", "Resource", "hash");
        var options = new CacheEntryOptions { Priority = CachePriority.High };
        var createdAt = DateTimeOffset.UtcNow;
        var expiresAt = createdAt.AddMinutes(1);

        var item = new CacheItem<string>(key, "value", createdAt, expiresAt, "application/json", options);

        Assert.Equal(key, item.Key);
        Assert.Equal("value", item.Value);
        Assert.Equal(createdAt, item.CreatedAt);
        Assert.Equal(expiresAt, item.ExpiresAt);
        Assert.Equal("application/json", item.ContentType);
        Assert.Equal(CachePriority.High, item.Options?.Priority);
    }
}
