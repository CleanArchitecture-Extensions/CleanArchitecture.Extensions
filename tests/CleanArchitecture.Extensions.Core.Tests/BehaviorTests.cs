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
        var options = Microsoft.Extensions.Options.Options.Create(new CoreExtensionsOptions { CorrelationIdFactory = () => "cid-logging" });
        var behavior = new LoggingBehavior<TestRequest, string>(logger, logContext, clock, options);

        var response = await behavior.Handle(new TestRequest(), _ => Task.FromResult("ok"), CancellationToken.None);

        Assert.Equal("ok", response);
        Assert.Equal(2, logger.Entries.Count(entry => entry.Level == LogLevel.Information));
    }

    /// <summary>
    /// Verifies logging pre-processor writes the start entry and seeds correlation.
    /// </summary>
    [Fact]
    public async Task LoggingPreProcessor_EmitsStartLog()
    {
        var logContext = new InMemoryLogContext();
        var clock = new FrozenClock(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var logger = new InMemoryAppLogger<TestRequest>(logContext, () => clock.UtcNow);
        var options = Microsoft.Extensions.Options.Options.Create(new CoreExtensionsOptions { CorrelationIdFactory = () => "cid-pre" });
        var preProcessor = new LoggingPreProcessor<TestRequest>(logger, logContext, clock, options);

        await preProcessor.Process(new TestRequest(), CancellationToken.None);

        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Debug && e.Message.Contains("Starting"));
        Assert.False(string.IsNullOrWhiteSpace(logContext.CorrelationId));
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
    /// Verifies performance logging can be disabled entirely.
    /// </summary>
    [Fact]
    public async Task PerformanceBehavior_SkipsLogging_WhenDisabled()
    {
        var logContext = new InMemoryLogContext();
        var options = Microsoft.Extensions.Options.Options.Create(new CoreExtensionsOptions
        {
            EnablePerformanceLogging = false,
            PerformanceWarningThreshold = TimeSpan.FromMilliseconds(1)
        });
        var clock = new FrozenClock();
        var logger = new InMemoryAppLogger<TestRequest>(logContext, () => clock.UtcNow);
        var behavior = new PerformanceBehavior<TestRequest, string>(logger, logContext, options, clock);

        var response = await behavior.Handle(new TestRequest(), _ => Task.FromResult("ok"), CancellationToken.None);

        Assert.Equal("ok", response);
        Assert.Empty(logger.Entries);
    }

    /// <summary>
    /// Verifies the correlation behavior preserves an existing correlation identifier.
    /// </summary>
    [Fact]
    public async Task CorrelationBehavior_KeepsExistingCorrelationId()
    {
        var logContext = new InMemoryLogContext { CorrelationId = "existing" };
        var options = Microsoft.Extensions.Options.Options.Create(new CoreExtensionsOptions { CorrelationIdFactory = () => "new" });
        var clock = new FrozenClock();
        var behavior = new CorrelationBehavior<TestRequest, string>(logContext, options, clock);

        var response = await behavior.Handle(new TestRequest(), _ => Task.FromResult("ok"), CancellationToken.None);

        Assert.Equal("ok", response);
        Assert.Equal("existing", logContext.CorrelationId);
    }

    /// <summary>
    /// Verifies the logging pre-processor prefers a resolver before generating a new correlation ID.
    /// </summary>
    [Fact]
    public async Task LoggingPreProcessor_UsesResolverFirst()
    {
        var logContext = new InMemoryLogContext();
        var clock = new FrozenClock(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var logger = new InMemoryAppLogger<TestRequest>(logContext, () => clock.UtcNow);
        var options = Microsoft.Extensions.Options.Options.Create(new CoreExtensionsOptions
        {
            CorrelationIdResolver = () => "resolved",
            CorrelationIdFactory = () => "generated"
        });
        var preProcessor = new LoggingPreProcessor<TestRequest>(logger, logContext, clock, options);

        await preProcessor.Process(new TestRequest(), CancellationToken.None);

        Assert.Equal("resolved", logContext.CorrelationId);
        Assert.Contains(logger.Entries, e => e.Properties?["CorrelationId"]?.ToString() == "resolved");
    }

    /// <summary>
    /// Verifies logging behavior falls back to the clock when no correlation factory is provided.
    /// </summary>
    [Fact]
    public async Task LoggingBehavior_UsesClockGuid_WhenFactoryMissing()
    {
        var logContext = new InMemoryLogContext();
        var clock = new DeterministicClock(new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"));
        var logger = new InMemoryAppLogger<TestRequest>(logContext, () => clock.UtcNow);
        var options = Microsoft.Extensions.Options.Options.Create(new CoreExtensionsOptions { CorrelationIdFactory = null! });
        var behavior = new LoggingBehavior<TestRequest, string>(logger, logContext, clock, options);

        var response = await behavior.Handle(new TestRequest(), _ => Task.FromResult("ok"), CancellationToken.None);

        Assert.Equal("ok", response);
        Assert.Equal("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee", logContext.CorrelationId);
        Assert.All(logger.Entries, e => Assert.Equal(logContext.CorrelationId, e.CorrelationId));
    }

    /// <summary>
    /// Test request used to validate pipeline behaviors.
    /// </summary>
    private sealed record TestRequest : IRequest<string>;

    private sealed class DeterministicClock : IClock
    {
        private readonly Guid _guid;
        private readonly DateTimeOffset _now;

        public DeterministicClock(Guid guid)
        {
            _guid = guid;
            _now = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        }

        public DateTimeOffset UtcNow => _now;
        public DateOnly Today => DateOnly.FromDateTime(_now.UtcDateTime);
        public long Timestamp => 0;
        public Guid NewGuid() => _guid;
        public Task Delay(TimeSpan delay, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
