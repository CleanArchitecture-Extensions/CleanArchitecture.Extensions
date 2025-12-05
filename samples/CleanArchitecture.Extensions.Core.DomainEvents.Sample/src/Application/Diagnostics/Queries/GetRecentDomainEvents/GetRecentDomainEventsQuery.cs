using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Application.Common.Interfaces;

namespace CleanArchitecture.Extensions.Core.DomainEvents.Sample.Application.Diagnostics.Queries.GetRecentDomainEvents;

public sealed record GetRecentDomainEventsQuery(int Count = 20) : IRequest<IReadOnlyCollection<DomainEventLogEntry>>;

public sealed class GetRecentDomainEventsQueryHandler : IRequestHandler<GetRecentDomainEventsQuery, IReadOnlyCollection<DomainEventLogEntry>>
{
    private readonly IDomainEventLog _domainEventLog;

    public GetRecentDomainEventsQueryHandler(IDomainEventLog domainEventLog)
    {
        _domainEventLog = domainEventLog;
    }

    public Task<IReadOnlyCollection<DomainEventLogEntry>> Handle(GetRecentDomainEventsQuery request, CancellationToken cancellationToken)
    {
        var take = Math.Clamp(request.Count, 1, 50);
        var entries = _domainEventLog.ReadRecent(take);
        return Task.FromResult(entries);
    }
}
