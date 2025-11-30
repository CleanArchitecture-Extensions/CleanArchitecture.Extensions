namespace CleanArchitecture.Extensions.Core.DomainEvents;

/// <summary>
/// Abstraction for dispatching domain events to interested handlers.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches a single domain event.
    /// </summary>
    /// <param name="domainEvent">Domain event to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the dispatch operation.</returns>
    Task DispatchAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches a collection of domain events.
    /// </summary>
    /// <param name="domainEvents">Domain events to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the dispatch operation.</returns>
    Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
