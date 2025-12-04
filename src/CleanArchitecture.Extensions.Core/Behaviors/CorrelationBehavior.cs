using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Options;
using CleanArchitecture.Extensions.Core.Time;
using Microsoft.Extensions.Options;
using MediatR;

namespace CleanArchitecture.Extensions.Core.Behaviors;

/// <summary>
/// MediatR behavior that ensures a correlation identifier is present in the logging context.
/// </summary>
public sealed class CorrelationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogContext _logContext;
    private readonly CoreExtensionsOptions _options;
    private readonly IClock _clock;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="logContext">Log context for correlation management.</param>
    /// <param name="options">Core extension options.</param>
    /// <param name="clock">Clock used for correlation ID generation fallback.</param>
    public CorrelationBehavior(ILogContext logContext, IOptions<CoreExtensionsOptions> options, IClock clock)
    {
        _logContext = logContext ?? throw new ArgumentNullException(nameof(logContext));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>
    /// Ensures a correlation identifier exists before invoking the next pipeline step.
    /// </summary>
    /// <param name="request">Incoming request.</param>
    /// <param name="next">Delegate to invoke the next handler in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response from the next handler.</returns>
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_logContext.CorrelationId))
        {
            _logContext.CorrelationId = (_options.CorrelationIdFactory ?? (() => _clock.NewGuid().ToString()))();
        }

        var correlationId = _logContext.CorrelationId;
        using var scope = _logContext.PushProperty("CorrelationId", correlationId);
        return next(cancellationToken);
    }
}
