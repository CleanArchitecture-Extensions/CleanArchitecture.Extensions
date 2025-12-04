using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Time;
using MediatR.Pipeline;

namespace CleanArchitecture.Extensions.Core.Behaviors;

/// <summary>
/// MediatR request pre-processor that emits a start log with correlation metadata.
/// </summary>
public sealed class LoggingPreProcessor<TRequest> : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    private readonly IAppLogger<TRequest> _logger;
    private readonly ILogContext _logContext;
    private readonly IClock _clock;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingPreProcessor{TRequest}"/> class.
    /// </summary>
    /// <param name="logger">Application logger.</param>
    /// <param name="logContext">Log context for correlation.</param>
    /// <param name="clock">Clock used for timestamps.</param>
    public LoggingPreProcessor(IAppLogger<TRequest> logger, ILogContext logContext, IClock clock)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logContext = logContext ?? throw new ArgumentNullException(nameof(logContext));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>
    /// Logs the start of request handling before pipeline behaviors execute.
    /// </summary>
    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var correlationId = EnsureCorrelationId();
        _logger.Debug($"Starting {typeof(TRequest).Name}", new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId,
            ["Timestamp"] = _clock.UtcNow
        });

        return Task.CompletedTask;
    }

    private string EnsureCorrelationId()
    {
        if (string.IsNullOrWhiteSpace(_logContext.CorrelationId))
        {
            _logContext.CorrelationId = _clock.NewGuid().ToString();
        }

        return _logContext.CorrelationId!;
    }
}
