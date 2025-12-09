using CleanArchitecture.Extensions.Core.Logging;

namespace CleanArchitecture.Extensions.Core.Tests;

/// <summary>
/// Tests covering log context and in-memory logger utilities.
/// </summary>
public class LoggingContextTests
{
    [Fact]
    public void InMemoryLogContext_PushProperty_AddsAndRemovesProperty()
    {
        var context = new InMemoryLogContext();

        using (context.PushProperty("Key", 123))
        {
            Assert.Equal(123, context.Properties["Key"]);
        }

        Assert.Empty(context.Properties);
    }

    [Fact]
    public void InMemoryLogContext_PushProperty_ThrowsWhenNameMissing()
    {
        var context = new InMemoryLogContext();

        Assert.Throws<ArgumentNullException>(() => context.PushProperty(null!, 1));
    }

    [Fact]
    public void InMemoryAppLogger_CapturesCorrelationAndTimestamp()
    {
        var context = new InMemoryLogContext { CorrelationId = "cid-logger" };
        var timestamp = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var logger = new InMemoryAppLogger<LoggingContextTests>(context, () => timestamp);

        logger.Log(LogLevel.Information, "message", properties: new Dictionary<string, object?> { ["Key"] = "Value" });

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(timestamp, entry.Timestamp);
        Assert.Equal("cid-logger", entry.CorrelationId);
        Assert.Equal("Value", entry.Properties?["Key"]);
    }

    [Fact]
    public void NoOpAppLogger_DoesNothing()
    {
        var logger = new NoOpAppLogger<LoggingContextTests>();

        var exception = Record.Exception(() => logger.Log(LogLevel.Critical, "noop"));

        Assert.Null(exception);
    }

    [Fact]
    public void NoOpLogContext_IgnoresPushedProperties()
    {
        var context = new NoOpLogContext { CorrelationId = "cid-noop" };

        using var scope = context.PushProperty("Ignored", 1);

        Assert.Equal("cid-noop", context.CorrelationId);
        scope.Dispose();
        Assert.Equal("cid-noop", context.CorrelationId);
    }
}
