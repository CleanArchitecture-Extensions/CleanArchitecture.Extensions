using CleanArchitecture.Extensions.Core.Time;

namespace CleanArchitecture.Extensions.Core.Time.Sample.Application.Diagnostics.Queries.GetTimeSnapshot;

public sealed record TimeSnapshotDto(DateTimeOffset UtcNow, DateOnly Today, long Timestamp, Guid Guid, string? CorrelationId);

public sealed record GetTimeSnapshotQuery : IRequest<TimeSnapshotDto>;

public sealed class GetTimeSnapshotQueryHandler : IRequestHandler<GetTimeSnapshotQuery, TimeSnapshotDto>
{
    private readonly IClock _clock;

    public GetTimeSnapshotQueryHandler(IClock clock)
    {
        _clock = clock;
    }

    public Task<TimeSnapshotDto> Handle(GetTimeSnapshotQuery request, CancellationToken cancellationToken)
    {
        var guid = _clock.NewGuid();
        var now = _clock.UtcNow;
        var today = _clock.Today;
        var timestamp = _clock.Timestamp;

        var snapshot = new TimeSnapshotDto(now, today, timestamp, guid, null);
        return Task.FromResult(snapshot);
    }
}
