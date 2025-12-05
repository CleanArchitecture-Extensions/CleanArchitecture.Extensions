using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Logging.Sample.Application.Common.Logging;

namespace CleanArchitecture.Extensions.Core.Logging.Sample.Application.Diagnostics.Queries.GetRecentLogs;

public sealed record GetRecentLogsQuery(int Count = 20) : IRequest<IReadOnlyCollection<LogEntry>>;

public sealed class GetRecentLogsQueryHandler : IRequestHandler<GetRecentLogsQuery, IReadOnlyCollection<LogEntry>>
{
    private readonly ILogRecorder _recorder;

    public GetRecentLogsQueryHandler(ILogRecorder recorder)
    {
        _recorder = recorder;
    }

    public Task<IReadOnlyCollection<LogEntry>> Handle(GetRecentLogsQuery request, CancellationToken cancellationToken)
    {
        var take = Math.Clamp(request.Count, 1, 50);
        var entries = _recorder.ReadRecent(take);
        return Task.FromResult(entries);
    }
}
