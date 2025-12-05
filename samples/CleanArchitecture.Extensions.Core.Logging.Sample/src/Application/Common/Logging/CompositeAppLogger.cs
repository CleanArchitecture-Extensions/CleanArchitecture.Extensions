using CleanArchitecture.Extensions.Core.Logging;
using Microsoft.Extensions.Logging;
using AppLogLevel = CleanArchitecture.Extensions.Core.Logging.LogLevel;

namespace CleanArchitecture.Extensions.Core.Logging.Sample.Application.Common.Logging;

public sealed class CompositeAppLogger<T> : IAppLogger<T>
{
    private readonly ILogger<T> _logger;
    private readonly ILogContext _logContext;
    private readonly ILogRecorder _recorder;

    public CompositeAppLogger(ILogger<T> logger, ILogContext logContext, ILogRecorder recorder)
    {
        _logger = logger;
        _logContext = logContext;
        _recorder = recorder;
    }

    public void Log(AppLogLevel level, string message, Exception? exception = null, IReadOnlyDictionary<string, object?>? properties = null)
    {
        var correlationId = _logContext.CorrelationId;

        using var correlationScope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId
        });

        IDisposable? propsScope = null;
        if (properties is { Count: > 0 })
        {
            propsScope = _logger.BeginScope(properties);
        }

        _logger.Log(MapLevel(level), exception, message);
        propsScope?.Dispose();

        _recorder.Record(new LogEntry(
            DateTimeOffset.UtcNow,
            level,
            message,
            correlationId,
            exception,
            properties));
    }

    private static Microsoft.Extensions.Logging.LogLevel MapLevel(AppLogLevel level) => level switch
    {
        AppLogLevel.Trace => Microsoft.Extensions.Logging.LogLevel.Trace,
        AppLogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
        AppLogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
        AppLogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
        AppLogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
        AppLogLevel.Critical => Microsoft.Extensions.Logging.LogLevel.Critical,
        _ => Microsoft.Extensions.Logging.LogLevel.None
    };
}
