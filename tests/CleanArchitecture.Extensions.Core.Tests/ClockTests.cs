using CleanArchitecture.Extensions.Core.Time;

namespace CleanArchitecture.Extensions.Core.Tests;

/// <summary>
/// Tests covering clock abstractions for deterministic time handling.
/// </summary>
public class ClockTests
{
    /// <summary>
    /// Ensures FrozenClock advances deterministically when instructed.
    /// </summary>
    [Fact]
    public void FrozenClock_Advance_MovesTimeForward()
    {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var clock = new FrozenClock(start);

        clock.Advance(TimeSpan.FromMinutes(5));

        Assert.Equal(start.AddMinutes(5), clock.UtcNow);
        Assert.Equal(TimeSpan.FromMinutes(5).Ticks, clock.Timestamp);
    }

    /// <summary>
    /// Ensures OffsetClock applies the configured offset to the underlying clock.
    /// </summary>
    [Fact]
    public void OffsetClock_AppliesOffset()
    {
        var innerClock = new FrozenClock(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero));
        var offsetClock = new OffsetClock(innerClock, TimeSpan.FromHours(1));

        Assert.Equal(innerClock.UtcNow.AddHours(1), offsetClock.UtcNow);
        Assert.Equal(innerClock.Timestamp, offsetClock.Timestamp);
    }

    /// <summary>
    /// Ensures the frozen clock Delay method advances time consistently.
    /// </summary>
    [Fact]
    public async Task FrozenClock_Delay_AdvancesTime()
    {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var clock = new FrozenClock(start);

        await clock.Delay(TimeSpan.FromMinutes(1));

        Assert.Equal(start.AddMinutes(1), clock.UtcNow);
        Assert.Equal(TimeSpan.FromMinutes(1).Ticks, clock.Timestamp);
    }

    /// <summary>
    /// Ensures the system clock reports the UTC date.
    /// </summary>
    [Fact]
    public void SystemClock_Today_UsesUtcDate()
    {
        var clock = new SystemClock();

        Assert.Equal(DateOnly.FromDateTime(clock.UtcNow.UtcDateTime), clock.Today);
    }
}
