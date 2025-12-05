using System.Collections.Concurrent;
using CleanArchitecture.Extensions.Core.DomainEvents;
using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Application.Common.Interfaces;

namespace CleanArchitecture.Extensions.Core.DomainEvents.Sample.Infrastructure.DomainEvents;

public sealed class InMemoryDomainEventLog : IDomainEventLog
{
    private const int MaxEntries = 50;
    private readonly ConcurrentQueue<DomainEventLogEntry> _entries = new();

    public void Record(DomainEvent domainEvent)
    {
        _entries.Enqueue(new DomainEventLogEntry(
            domainEvent.Id,
            domainEvent.GetType().Name,
            domainEvent.CorrelationId,
            domainEvent.OccurredOnUtc,
            DateTimeOffset.UtcNow));

        while (_entries.Count > MaxEntries && _entries.TryDequeue(out _))
        {
            // Trim to the latest entries to keep memory bounded.
        }
    }

    public IReadOnlyCollection<DomainEventLogEntry> ReadRecent(int take = MaxEntries)
    {
        var items = _entries.ToArray();
        return items
            .Reverse()
            .Take(Math.Clamp(take, 1, MaxEntries))
            .ToArray();
    }
}
