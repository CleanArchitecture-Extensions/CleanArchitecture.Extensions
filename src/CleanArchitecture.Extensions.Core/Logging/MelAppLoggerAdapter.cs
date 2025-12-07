using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Extensions.Core.Logging;

/// <summary>
/// Adapts <see cref="ILogger{TCategoryName}"/> to the <see cref="IAppLogger{T}"/> abstraction while preserving correlation scopes.
/// </summary>
/// <typeparam name="T">Category type.</typeparam>
public sealed class MelAppLoggerAdapter<T> : IAppLogger<T>
{
    private readonly ILogger<T> _logger;
    private readonly ILogContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="MelAppLoggerAdapter{T}"/> class.
    /// </summary>
    /// <param name="logger">Microsoft.Extensions.Logging logger.</param>
    /// <param name="context">Log context that supplies correlation identifiers.</param>
    public MelAppLoggerAdapter(ILogger<T> logger, ILogContext context)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public void Log(LogLevel level, string message, Exception? exception = null, IReadOnlyDictionary<string, object?>? properties = null)
    {
        var mappedLevel = Map(level);
        if (!_logger.IsEnabled(mappedLevel))
        {
            return;
        }

        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = _context.CorrelationId
        });

        if (properties is null || properties.Count == 0)
        {
            _logger.Log(mappedLevel, exception, "{Message}", message);
            return;
        }

        _logger.Log(mappedLevel, exception, "{Message} {@Properties}", message, properties);
    }

    private static Microsoft.Extensions.Logging.LogLevel Map(LogLevel level) => level switch
    {
        LogLevel.Trace => Microsoft.Extensions.Logging.LogLevel.Trace,
        LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
        LogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
        LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
        LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
        LogLevel.Critical => Microsoft.Extensions.Logging.LogLevel.Critical,
        _ => Microsoft.Extensions.Logging.LogLevel.None
    };
}
