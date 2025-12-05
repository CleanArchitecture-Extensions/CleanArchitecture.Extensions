using CleanArchitecture.Extensions.Core.Logging.Sample.Application.Common.Logging;
using CleanArchitecture.Extensions.Core.Logging;

namespace CleanArchitecture.Extensions.Core.Logging.Sample.Application.Diagnostics.Commands.EmitLogPulse;

public sealed record EmitLogPulseCommand(string? Message, bool IncludeWarning = false, bool IncludeError = false) : IRequest;

public sealed class EmitLogPulseCommandHandler : IRequestHandler<EmitLogPulseCommand>
{
    private readonly IAppLogger<EmitLogPulseCommandHandler> _logger;
    private readonly ILogContext _logContext;

    public EmitLogPulseCommandHandler(IAppLogger<EmitLogPulseCommandHandler> logger, ILogContext logContext)
    {
        _logger = logger;
        _logContext = logContext;
    }

    public Task Handle(EmitLogPulseCommand request, CancellationToken cancellationToken)
    {
        var correlationId = _logContext.CorrelationId ?? Guid.NewGuid().ToString("N");
        _logContext.CorrelationId = correlationId;

        using var featureScope = _logContext.PushProperty("Feature", "LoggingSample");

        var properties = new Dictionary<string, object?>
        {
            ["MessageLength"] = request.Message?.Length ?? 0,
            ["IncludeWarning"] = request.IncludeWarning,
            ["IncludeError"] = request.IncludeError
        };

        _logger.Log(LogLevel.Information, $"Received log pulse: {request.Message ?? "(no message)"}", properties: properties);

        if (request.IncludeWarning)
        {
            _logger.Log(LogLevel.Warning, "Sample warning emitted for diagnostics", properties: properties);
        }

        if (request.IncludeError)
        {
            _logger.Log(LogLevel.Error, "Sample error emitted for diagnostics", properties: properties);
        }

        return Task.CompletedTask;
    }
}
