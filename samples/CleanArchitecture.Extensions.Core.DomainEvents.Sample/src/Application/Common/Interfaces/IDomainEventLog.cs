using CleanArchitecture.Extensions.Core.DomainEvents;

namespace CleanArchitecture.Extensions.Core.DomainEvents.Sample.Application.Common.Interfaces;

public interface IDomainEventLog
{
    void Record(DomainEvent domainEvent);

    IReadOnlyCollection<DomainEventLogEntry> ReadRecent(int take = 20);
}

public sealed record DomainEventLogEntry(
    Guid Id,
    string Type,
    string? CorrelationId,
    DateTimeOffset OccurredOnUtc,
    DateTimeOffset RecordedOnUtc);
