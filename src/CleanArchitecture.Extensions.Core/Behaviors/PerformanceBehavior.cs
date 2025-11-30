using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Options;
using CleanArchitecture.Extensions.Core.Time;
using MediatR;

namespace CleanArchitecture.Extensions.Core.Behaviors;

/// <summary>
/// MediatR behavior that measures request execution time and emits performance logs.
/// </summary>
public sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IAppLogger<TRequest> _logger;
    private readonly ILogContext _logContext;
    private readonly CoreExtensionsOptions _options;
    private readonly IClock _clock;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="logger">Application logger.</param>
    /// <param name="logContext">Log context containing correlation metadata.</param>
    /// <param name="options">Performance logging options.</param>
    /// <param name="clock">Clock used for timing operations.</param>
    public PerformanceBehavior(IAppLogger<TRequest> logger, ILogContext logContext, CoreExtensionsOptions options, IClock clock)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logContext = logContext ?? throw new ArgumentNullException(nameof(logContext));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>
    /// Measures the execution time of the next pipeline step and logs warnings when thresholds are exceeded.
    /// </summary>
    /// <param name="request">Incoming request.</param>
    /// <param name="next">Delegate to invoke the next handler in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response from the next handler.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_options.EnablePerformanceLogging)
        {
            return await next().ConfigureAwait(false);
        }

        var start = _clock.UtcNow;
        var response = await next().ConfigureAwait(false);
        var elapsed = _clock.UtcNow - start;

        var properties = new Dictionary<string, object?>
        {
            ["RequestType"] = typeof(TRequest).FullName ?? typeof(TRequest).Name,
            ["CorrelationId"] = _logContext.CorrelationId,
            ["ElapsedMilliseconds"] = elapsed.TotalMilliseconds
        };

        if (elapsed > _options.PerformanceWarningThreshold)
        {
            _logger.Warn($"Long running request: {typeof(TRequest).Name} ({elapsed.TotalMilliseconds:0} ms)", properties);
        }
        else
        {
            _logger.Debug($"Completed {typeof(TRequest).Name} in {elapsed.TotalMilliseconds:0} ms", properties);
        }

        return response;
    }
}
