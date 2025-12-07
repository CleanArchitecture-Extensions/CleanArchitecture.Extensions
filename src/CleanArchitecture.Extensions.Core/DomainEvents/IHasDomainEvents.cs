namespace CleanArchitecture.Extensions.Core.DomainEvents;

/// <summary>
/// Defines an aggregate that exposes domain events for dispatch.
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// Gets the currently tracked domain events.
    /// </summary>
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }

    /// <summary>
    /// Drains and clears tracked domain events.
    /// </summary>
    /// <returns>Events that were tracked before the drain.</returns>
    IReadOnlyCollection<DomainEvent> DequeueDomainEvents();
}
