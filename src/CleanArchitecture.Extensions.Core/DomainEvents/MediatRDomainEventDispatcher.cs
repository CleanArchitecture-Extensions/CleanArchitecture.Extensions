using MediatR;

namespace CleanArchitecture.Extensions.Core.DomainEvents;

/// <summary>
/// Publishes domain events to MediatR notification handlers.
/// </summary>
public sealed class MediatRDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediatRDomainEventDispatcher"/> class.
    /// </summary>
    /// <param name="mediator">MediatR mediator used for publishing events.</param>
    public MediatRDomainEventDispatcher(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <inheritdoc />
    public Task DispatchAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        return _mediator.Publish(domainEvent, cancellationToken);
    }

    /// <inheritdoc />
    public Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);
        var publishes = domainEvents.Select(domainEvent => _mediator.Publish(domainEvent, cancellationToken));
        return Task.WhenAll(publishes);
    }
}
