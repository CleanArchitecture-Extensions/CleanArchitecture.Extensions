namespace CleanArchitecture.Extensions.Core.DomainEvents;

/// <summary>
/// Helpers for dispatching domain events from aggregates.
/// </summary>
public static class DomainEventDispatcherExtensions
{
    /// <summary>
    /// Dispatches and clears domain events from the provided aggregates.
    /// </summary>
    /// <param name="dispatcher">Dispatcher to use.</param>
    /// <param name="sources">Aggregates exposing domain events.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task DispatchAndClearAsync(
        this IDomainEventDispatcher dispatcher,
        IEnumerable<IHasDomainEvents> sources,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(sources);

        var domainEvents = sources
            .Where(source => source is not null)
            .SelectMany(source => source.DequeueDomainEvents())
            .ToArray();

        if (domainEvents.Length == 0)
        {
            return;
        }

        await dispatcher.DispatchAsync(domainEvents, cancellationToken).ConfigureAwait(false);
    }
}
