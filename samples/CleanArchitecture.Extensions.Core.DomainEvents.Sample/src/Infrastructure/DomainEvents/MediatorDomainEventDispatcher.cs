using CleanArchitecture.Extensions.Core.DomainEvents;
using MediatR;

namespace CleanArchitecture.Extensions.Core.DomainEvents.Sample.Infrastructure.DomainEvents;

public sealed class MediatorDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;

    public MediatorDomainEventDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task DispatchAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default) =>
        _mediator.Publish(domainEvent, cancellationToken);

    public async Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }
}
