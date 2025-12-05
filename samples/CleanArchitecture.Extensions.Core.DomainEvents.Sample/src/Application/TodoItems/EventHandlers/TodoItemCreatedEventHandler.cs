using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Domain.Events;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Extensions.Core.DomainEvents.Sample.Application.TodoItems.EventHandlers;

public class TodoItemCreatedEventHandler : INotificationHandler<TodoItemCreatedEvent>
{
    private readonly ILogger<TodoItemCreatedEventHandler> _logger;

    public TodoItemCreatedEventHandler(ILogger<TodoItemCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(TodoItemCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Domain event {DomainEvent} handled for item {ItemId} with correlation {CorrelationId}",
            notification.GetType().Name,
            notification.Item.Id,
            notification.CorrelationId ?? "(none)");

        return Task.CompletedTask;
    }
}
