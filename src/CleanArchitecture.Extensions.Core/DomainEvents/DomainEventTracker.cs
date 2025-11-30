namespace CleanArchitecture.Extensions.Core.DomainEvents;

/// <summary>
/// Tracks domain events raised by aggregates until they are dispatched.
/// </summary>
public sealed class DomainEventTracker
{
    private readonly List<DomainEvent> _events = new();

    /// <summary>
    /// Gets the currently tracked domain events.
    /// </summary>
    public IReadOnlyCollection<DomainEvent> Events => _events.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the tracker.
    /// </summary>
    /// <param name="domainEvent">Event to add.</param>
    public void Add(DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _events.Add(domainEvent);
    }

    /// <summary>
    /// Gets a value indicating whether any events are currently tracked.
    /// </summary>
    public bool HasEvents => _events.Count > 0;

    /// <summary>
    /// Drains all tracked events, returning them and clearing the tracker.
    /// </summary>
    /// <returns>Snapshot of events at the time of draining.</returns>
    public IReadOnlyCollection<DomainEvent> Drain()
    {
        var snapshot = _events.ToArray();
        _events.Clear();
        return snapshot;
    }

    /// <summary>
    /// Clears all tracked events without returning them.
    /// </summary>
    public void Clear() => _events.Clear();
}
