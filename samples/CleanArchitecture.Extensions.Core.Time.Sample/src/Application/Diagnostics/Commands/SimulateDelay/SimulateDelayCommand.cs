using System.Diagnostics;
using CleanArchitecture.Extensions.Core.Time;

namespace CleanArchitecture.Extensions.Core.Time.Sample.Application.Diagnostics.Commands.SimulateDelay;

public sealed record DelayResult(DateTimeOffset StartedAtUtc, DateTimeOffset EndedAtUtc, TimeSpan RequestedDelay, TimeSpan ObservedDelay);

public sealed record SimulateDelayCommand(int Milliseconds) : IRequest<DelayResult>;

public sealed class SimulateDelayCommandHandler : IRequestHandler<SimulateDelayCommand, DelayResult>
{
    private readonly IClock _clock;

    public SimulateDelayCommandHandler(IClock clock)
    {
        _clock = clock;
    }

    public async Task<DelayResult> Handle(SimulateDelayCommand request, CancellationToken cancellationToken)
    {
        var delay = TimeSpan.FromMilliseconds(Math.Max(0, request.Milliseconds));
        var started = _clock.UtcNow;

        await _clock.Delay(delay, cancellationToken);

        var ended = _clock.UtcNow;
        var observed = ended - started;

        return new DelayResult(started, ended, delay, observed);
    }
}
