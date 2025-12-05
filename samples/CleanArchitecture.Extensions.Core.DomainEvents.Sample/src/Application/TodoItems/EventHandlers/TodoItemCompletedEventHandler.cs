using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Domain.Events;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Extensions.Core.DomainEvents.Sample.Application.TodoItems.EventHandlers;

public class TodoItemCompletedEventHandler : INotificationHandler<TodoItemCompletedEvent>
{
    private readonly ILogger<TodoItemCompletedEventHandler> _logger;

    public TodoItemCompletedEventHandler(ILogger<TodoItemCompletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(TodoItemCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Domain event {DomainEvent} handled for item {ItemId} with correlation {CorrelationId}",
            notification.GetType().Name,
            notification.Item.Id,
            notification.CorrelationId ?? "(none)");

        return Task.CompletedTask;
    }
}
