using CleanArchitecture.Extensions.Caching.Internal;

namespace CleanArchitecture.Extensions.Caching.Tests;

public class KeyedLockTests
{
    [Fact]
    public async Task TryAcquireAsync_releases_waiting_entries_without_disposing_them()
    {
        var keyedLock = new KeyedLock();
        var first = await keyedLock.TryAcquireAsync("resource", TimeSpan.Zero, CancellationToken.None);
        Assert.NotNull(first);

        var waiting = keyedLock.TryAcquireAsync("resource", TimeSpan.FromSeconds(5), CancellationToken.None).AsTask();

        first.Value.Dispose();
        var second = await waiting;

        Assert.NotNull(second);
        second.Value.Dispose();
    }

    [Fact]
    public async Task TryAcquireAsync_can_reuse_key_after_entry_cleanup()
    {
        var keyedLock = new KeyedLock();
        var first = await keyedLock.TryAcquireAsync("resource", TimeSpan.Zero, CancellationToken.None);
        Assert.NotNull(first);
        first.Value.Dispose();

        var second = await keyedLock.TryAcquireAsync("resource", TimeSpan.Zero, CancellationToken.None);

        Assert.NotNull(second);
        second.Value.Dispose();
    }

    [Fact]
    public async Task TryAcquireAsync_cleans_up_timed_out_waiters()
    {
        var keyedLock = new KeyedLock();
        var first = await keyedLock.TryAcquireAsync("resource", TimeSpan.Zero, CancellationToken.None);
        Assert.NotNull(first);

        var timedOut = await keyedLock.TryAcquireAsync("resource", TimeSpan.Zero, CancellationToken.None);

        Assert.Null(timedOut);
        first.Value.Dispose();
    }
}
