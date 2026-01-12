namespace CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Domain.Events;

public class TodoItemCreatedEvent : BaseEvent
{
    public TodoItemCreatedEvent(TodoItem item)
    {
        Item = item;
    }

    public TodoItem Item { get; }
}
