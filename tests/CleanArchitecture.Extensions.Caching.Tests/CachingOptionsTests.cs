using CleanArchitecture.Extensions.Caching.Options;

namespace CleanArchitecture.Extensions.Caching.Tests;

public class CachingOptionsTests
{
    [Fact]
    public void Default_options_set_expected_defaults()
    {
        var options = CachingOptions.Default;

        Assert.True(options.Enabled);
        Assert.Equal("CleanArchitectureExtensions", options.DefaultNamespace);
        Assert.NotNull(options.DefaultEntryOptions);
        Assert.Equal(CachePriority.Normal, options.DefaultEntryOptions.Priority);
        Assert.NotNull(options.StampedePolicy);
        Assert.Null(options.PreferredSerializer);
    }

    [Fact]
    public void Cache_entry_options_defaults_are_null_and_normal_priority()
    {
        var options = new CacheEntryOptions();

        Assert.Null(options.AbsoluteExpiration);
        Assert.Null(options.AbsoluteExpirationRelativeToNow);
        Assert.Null(options.SlidingExpiration);
        Assert.Null(options.Size);
        Assert.Equal(CachePriority.Normal, options.Priority);
    }

    [Fact]
    public void Stampede_policy_defaults_enable_locking_with_timeout_and_jitter()
    {
        var policy = CacheStampedePolicy.Default;

        Assert.True(policy.EnableLocking);
        Assert.Equal(TimeSpan.FromSeconds(5), policy.LockTimeout);
        Assert.Equal(TimeSpan.FromMilliseconds(50), policy.Jitter);
    }
}
