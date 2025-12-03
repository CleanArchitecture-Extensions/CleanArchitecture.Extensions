using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Time;

namespace CleanArchitecture.Extensions.Core.Pipeline.Sample.Application.Diagnostics.Commands.SimulateWork;

public sealed record SimulateWorkCommand(int Milliseconds) : IRequest<Unit>;

public sealed class SimulateWorkCommandHandler : IRequestHandler<SimulateWorkCommand, Unit>
{
    private readonly IClock _clock;
    private readonly IAppLogger<SimulateWorkCommandHandler> _logger;

    public SimulateWorkCommandHandler(IClock clock, IAppLogger<SimulateWorkCommandHandler> logger)
    {
        _clock = clock;
        _logger = logger;
    }

    public async Task<Unit> Handle(SimulateWorkCommand request, CancellationToken cancellationToken)
    {
        var delay = Math.Max(0, request.Milliseconds);
        _logger.Log(LogLevel.Information, $"Simulating {delay} ms of work");

        await _clock.Delay(TimeSpan.FromMilliseconds(delay), cancellationToken);

        _logger.Log(LogLevel.Debug, $"Completed simulated work after {delay} ms");
        return Unit.Value;
    }
}
