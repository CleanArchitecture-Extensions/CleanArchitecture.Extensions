using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Time;

namespace CleanArchitecture.Extensions.Core.Pipeline.Sample.Application.Diagnostics.Queries.GetPipelineDiagnostics;

public sealed record GetPipelineDiagnosticsQuery : IRequest<PipelineDiagnosticsDto>;

public sealed record PipelineDiagnosticsDto(string CorrelationId, DateTimeOffset TimestampUtc);

public sealed class GetPipelineDiagnosticsQueryHandler : IRequestHandler<GetPipelineDiagnosticsQuery, PipelineDiagnosticsDto>
{
    private readonly ILogContext _logContext;
    private readonly IClock _clock;
    private readonly IAppLogger<GetPipelineDiagnosticsQueryHandler> _logger;

    public GetPipelineDiagnosticsQueryHandler(ILogContext logContext, IClock clock, IAppLogger<GetPipelineDiagnosticsQueryHandler> logger)
    {
        _logContext = logContext;
        _clock = clock;
        _logger = logger;
    }

    public Task<PipelineDiagnosticsDto> Handle(GetPipelineDiagnosticsQuery request, CancellationToken cancellationToken)
    {
        var correlationId = _logContext.CorrelationId ?? _clock.NewGuid().ToString("N");

        _logger.Log(LogLevel.Information, $"Diagnostics requested with correlation {correlationId}");

        return Task.FromResult(new PipelineDiagnosticsDto(correlationId, _clock.UtcNow));
    }
}
