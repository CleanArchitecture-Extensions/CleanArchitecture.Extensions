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

    private sealed class MelAppLoggerAdapter<T> : IAppLogger<T>
    {
        private readonly ILogger<T> _logger;
        private readonly ILogContext _context;

        public MelAppLoggerAdapter(ILogger<T> logger, ILogContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void Log(CoreLogLevel level, string message, Exception? exception = null, IReadOnlyDictionary<string, object?>? properties = null)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object?>
            {
                ["CorrelationId"] = _context.CorrelationId
            });

            _logger.Log(Map(level), exception, message + " {@props}", properties);
        }

        private static Microsoft.Extensions.Logging.LogLevel Map(CoreLogLevel level) => level switch
        {
            CoreLogLevel.Trace => Microsoft.Extensions.Logging.LogLevel.Trace,
            CoreLogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
            CoreLogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
            CoreLogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
            CoreLogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            CoreLogLevel.Critical => Microsoft.Extensions.Logging.LogLevel.Critical,
            _ => Microsoft.Extensions.Logging.LogLevel.None
        };
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
