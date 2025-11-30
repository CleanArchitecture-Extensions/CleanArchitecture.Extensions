using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Time;
using MediatR;
using MediatR.Pipeline;

namespace CleanArchitecture.Extensions.Core.Behaviors;

/// <summary>
/// MediatR behavior that records request lifecycle events and ensures correlation scope.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>, IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    private readonly IAppLogger<TRequest> _logger;
    private readonly ILogContext _logContext;
    private readonly IClock _clock;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="logger">Application logger.</param>
    /// <param name="logContext">Log context for correlation.</param>
    /// <param name="clock">Clock used for timestamps.</param>
    public LoggingBehavior(IAppLogger<TRequest> logger, ILogContext logContext, IClock clock)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logContext = logContext ?? throw new ArgumentNullException(nameof(logContext));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>
    /// Pre-processor hook to log the start of request handling.
    /// </summary>
    /// <param name="request">Incoming request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A completed task.</returns>
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

    /// <summary>
    /// Handles the request by logging before and after execution.
    /// </summary>
    /// <param name="request">Incoming request.</param>
    /// <param name="next">Delegate to invoke the next handler in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response from the next handler.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var correlationId = EnsureCorrelationId();
        using var scope = _logContext.PushProperty("CorrelationId", correlationId);

        _logger.Info($"Handling {typeof(TRequest).Name}", new Dictionary<string, object?>
        {
            ["RequestType"] = typeof(TRequest).FullName ?? typeof(TRequest).Name,
            ["CorrelationId"] = correlationId
        });

        var response = await next().ConfigureAwait(false);

        _logger.Info($"Handled {typeof(TRequest).Name}", new Dictionary<string, object?>
        {
            ["RequestType"] = typeof(TRequest).FullName ?? typeof(TRequest).Name,
            ["CorrelationId"] = correlationId
        });

        return response;
    }

    /// <summary>
    /// Ensures a correlation identifier exists in the log context, creating one when missing.
    /// </summary>
    /// <returns>The existing or newly generated correlation identifier.</returns>
    private string EnsureCorrelationId()
    {
        if (string.IsNullOrWhiteSpace(_logContext.CorrelationId))
        {
            _logContext.CorrelationId = _clock.NewGuid().ToString();
        }

        return _logContext.CorrelationId!;
    }
}
