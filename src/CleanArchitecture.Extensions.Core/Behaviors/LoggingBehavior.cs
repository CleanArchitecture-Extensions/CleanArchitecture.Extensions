using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Options;
using CleanArchitecture.Extensions.Core.Time;
using Microsoft.Extensions.Options;
using MediatR;

namespace CleanArchitecture.Extensions.Core.Behaviors;

/// <summary>
/// MediatR behavior that records request lifecycle events and ensures correlation scope.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IAppLogger<TRequest> _logger;
    private readonly ILogContext _logContext;
    private readonly IClock _clock;
    private readonly CoreExtensionsOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="logger">Application logger.</param>
    /// <param name="logContext">Log context for correlation.</param>
    /// <param name="clock">Clock used for timestamps.</param>
    /// <param name="options">Options controlling correlation ID generation and logging behavior.</param>
    public LoggingBehavior(IAppLogger<TRequest> logger, ILogContext logContext, IClock clock, IOptions<CoreExtensionsOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logContext = logContext ?? throw new ArgumentNullException(nameof(logContext));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
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

        _logger.Debug($"Starting {typeof(TRequest).Name}", new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId,
            ["Timestamp"] = _clock.UtcNow
        });

        _logger.Info($"Handling {typeof(TRequest).Name}", new Dictionary<string, object?>
        {
            ["RequestType"] = typeof(TRequest).FullName ?? typeof(TRequest).Name,
            ["CorrelationId"] = correlationId
        });

        var response = await next(cancellationToken).ConfigureAwait(false);

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
            var factory = _options.CorrelationIdFactory ?? (() => _clock.NewGuid().ToString());
            _logContext.CorrelationId = factory();
        }

        return _logContext.CorrelationId!;
    }
}
