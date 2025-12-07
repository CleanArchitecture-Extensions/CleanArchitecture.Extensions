using CoreLogLevel = CleanArchitecture.Extensions.Core.Logging.LogLevel;
using CleanArchitecture.Extensions.Core.Logging;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Extensions.Core.Tests;

/// <summary>
/// Tests covering logging adapters to provider-specific loggers.
/// </summary>
public class LoggingAdapterTests
{
    /// <summary>
    /// Verifies that a MEL adapter propagates correlation into scopes and logs properties.
    /// </summary>
    [Fact]
    public void MelAdapter_PropagatesCorrelationAndProperties()
    {
        var logContext = new InMemoryLogContext { CorrelationId = "cid-123" };
        var sink = new TestLogger<LoggingAdapterTests>();
        var adapter = new MelAppLoggerAdapter<LoggingAdapterTests>(sink, logContext);

        adapter.Log(CoreLogLevel.Information, "hello", properties: new Dictionary<string, object?> { ["Key"] = "Value" });

        Assert.Contains(sink.Entries, e => e.Message.Contains("hello"));
        var scope = Assert.IsType<Dictionary<string, object?>>(sink.ScopeState);
        Assert.Equal("cid-123", scope["CorrelationId"]);
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        private sealed class ScopeCapture<TState>(TState state) : IDisposable
        {
            public TState State { get; } = state;
            public void Dispose()
            {
            }
        }

        public List<(Microsoft.Extensions.Logging.LogLevel Level, string Message, object? State, Exception? Exception)> Entries { get; } = new();
        public object? ScopeState { get; private set; }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            ScopeState = state;
            return new ScopeCapture<TState>(state);
        }

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, formatter(state, exception), state, exception));
        }
    }
}
