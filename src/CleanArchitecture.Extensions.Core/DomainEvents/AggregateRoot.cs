namespace CleanArchitecture.Extensions.Core.DomainEvents;

/// <summary>
/// Base type for aggregate roots that collect domain events for later dispatch.
/// </summary>
public abstract class AggregateRoot : IHasDomainEvents
{
    private readonly DomainEventTracker _domainEvents = new();

    /// <summary>
    /// Gets the domain events raised by this aggregate.
    /// </summary>
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.Events;

    /// <summary>
    /// Raises a domain event and tracks it for dispatch.
    /// </summary>
    /// <param name="domainEvent">Domain event to raise.</param>
    protected void RaiseDomainEvent(DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Drains all tracked domain events and clears them.
    /// </summary>
    /// <returns>Snapshot of events at the time of draining.</returns>
    public IReadOnlyCollection<DomainEvent> DequeueDomainEvents() => _domainEvents.Drain();

    /// <summary>
    /// Clears all tracked domain events without dispatching them.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
