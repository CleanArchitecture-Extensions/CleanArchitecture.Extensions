using CleanArchitecture.Extensions.Core.Time;
using CleanArchitecture.Extensions.Core.Time.Sample.Application.Diagnostics.Commands.SimulateDelay;
using CleanArchitecture.Extensions.Core.Time.Sample.Application.Diagnostics.Queries.GetTimeSnapshot;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Extensions.Core.Time.Sample.Application.UnitTests.Diagnostics;

public class TimeDiagnosticsTests
{
    [Test]
    public async Task GetTimeSnapshot_UsesFrozenClock()
    {
        var fixedTime = new DateTimeOffset(2025, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var clock = new FrozenClock(fixedTime);
        var handler = new GetTimeSnapshotQueryHandler(clock);

        var result = await handler.Handle(new GetTimeSnapshotQuery(), CancellationToken.None);

        result.UtcNow.ShouldBe(fixedTime);
        result.Today.ShouldBe(DateOnly.FromDateTime(fixedTime.UtcDateTime));
        result.Guid.ShouldNotBe(default);
    }

    [Test]
    public async Task SimulateDelay_AdvancesFrozenClockWithoutSleeping()
    {
        var fixedTime = new DateTimeOffset(2025, 2, 3, 4, 5, 6, TimeSpan.Zero);
        var clock = new FrozenClock(fixedTime);
        var handler = new SimulateDelayCommandHandler(clock);

        var result = await handler.Handle(new SimulateDelayCommand(250), CancellationToken.None);

        result.StartedAtUtc.ShouldBe(fixedTime);
        result.EndedAtUtc.ShouldBe(fixedTime.AddMilliseconds(250));
        result.ObservedDelay.ShouldBe(TimeSpan.FromMilliseconds(250));
    }
}
