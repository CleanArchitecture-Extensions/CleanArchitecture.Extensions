using CleanArchitecture.Extensions.Core.DomainEvents;
using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Application.Common.Interfaces;

namespace CleanArchitecture.Extensions.Core.DomainEvents.Sample.Infrastructure.DomainEvents;

public sealed class RecordingDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IDomainEventDispatcher _inner;
    private readonly IDomainEventLog _domainEventLog;

    public RecordingDomainEventDispatcher(IDomainEventDispatcher inner, IDomainEventLog domainEventLog)
    {
        _inner = inner;
        _domainEventLog = domainEventLog;
    }

    public async Task DispatchAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _inner.DispatchAsync(domainEvent, cancellationToken);
        _domainEventLog.Record(domainEvent);
    }

    public async Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await DispatchAsync(domainEvent, cancellationToken);
        }
    }
}
