using System.Linq;
using CleanArchitecture.Extensions.Core.Behaviors;
using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Options;
using CleanArchitecture.Extensions.Core.Time;
using Microsoft.Extensions.Options;
using MediatR;

namespace CleanArchitecture.Extensions.Core.Tests;

/// <summary>
/// Tests covering MediatR pipeline behaviors for correlation, logging, and performance.
/// </summary>
public class BehaviorTests
{
    /// <summary>
    /// Verifies correlation behavior assigns a correlation identifier when missing.
    /// </summary>
    [Fact]
    public async Task CorrelationBehavior_SetsCorrelationId_WhenMissing()
    {
        var logContext = new InMemoryLogContext();
        var options = Microsoft.Extensions.Options.Options.Create(new CoreExtensionsOptions { CorrelationIdFactory = () => "cid-test" });
        var clock = new FrozenClock();
        var behavior = new CorrelationBehavior<TestRequest, string>(logContext, options, clock);

        var response = await behavior.Handle(new TestRequest(), _ => Task.FromResult("ok"), CancellationToken.None);

        Assert.Equal("ok", response);
        Assert.Equal("cid-test", logContext.CorrelationId);
    }

    /// <summary>
    /// Verifies logging behavior writes start and finish log entries.
    /// </summary>
    [Fact]
    public async Task LoggingBehavior_WritesStartAndFinishEntries()
    {
        var logContext = new InMemoryLogContext { CorrelationId = "cid-logging" };
        var clock = new FrozenClock(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var logger = new InMemoryAppLogger<TestRequest>(logContext, () => clock.UtcNow);
        var behavior = new LoggingBehavior<TestRequest, string>(logger, logContext, clock);

        var response = await behavior.Handle(new TestRequest(), _ => Task.FromResult("ok"), CancellationToken.None);

        Assert.Equal("ok", response);
        Assert.Equal(2, logger.Entries.Count(entry => entry.Level == LogLevel.Information));
    }

    /// <summary>
    /// Verifies performance behavior emits warnings when the threshold is exceeded.
    /// </summary>
    [Fact]
    public async Task PerformanceBehavior_Warns_WhenThresholdExceeded()
    {
        var logContext = new InMemoryLogContext { CorrelationId = "cid-perf" };
        var options = Microsoft.Extensions.Options.Options.Create(new CoreExtensionsOptions
        {
            PerformanceWarningThreshold = TimeSpan.FromMilliseconds(10),
            EnablePerformanceLogging = true
        });
        var clock = new FrozenClock();
        var logger = new InMemoryAppLogger<TestRequest>(logContext, () => clock.UtcNow);
        var behavior = new PerformanceBehavior<TestRequest, string>(logger, logContext, options, clock);

        var response = await behavior.Handle(
            new TestRequest(),
            _ =>
            {
                clock.Advance(TimeSpan.FromMilliseconds(25));
                return Task.FromResult("ok");
            },
            CancellationToken.None);

        Assert.Equal("ok", response);
        Assert.Contains(logger.Entries, entry => entry.Level == LogLevel.Warning);
    }

    /// <summary>
    /// Test request used to validate pipeline behaviors.
    /// </summary>
    private sealed record TestRequest : IRequest<string>;
}
