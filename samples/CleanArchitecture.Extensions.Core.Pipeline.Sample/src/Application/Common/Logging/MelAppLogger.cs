using CleanArchitecture.Extensions.Core.Logging;
using Microsoft.Extensions.Logging;
using AppLogLevel = CleanArchitecture.Extensions.Core.Logging.LogLevel;

namespace CleanArchitecture.Extensions.Core.Pipeline.Sample.Application.Common.Logging;

public sealed class MelAppLogger<T> : IAppLogger<T>
{
    private readonly ILogger<T> _logger;
    private readonly ILogContext _logContext;

    public MelAppLogger(ILogger<T> logger, ILogContext logContext)
    {
        _logger = logger;
        _logContext = logContext;
    }

    public void Log(AppLogLevel level, string message, Exception? exception = null, IReadOnlyDictionary<string, object?>? properties = null)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = _logContext.CorrelationId
        });

        if (properties is not null && properties.Count > 0)
        {
            using var propsScope = _logger.BeginScope(properties);
            _logger.Log(MapLevel(level), exception, message);
        }
        else
        {
            _logger.Log(MapLevel(level), exception, message);
        }
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
