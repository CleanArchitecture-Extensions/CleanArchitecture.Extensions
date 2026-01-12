namespace CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Domain.Events;

public class TodoItemDeletedEvent : BaseEvent
{
    public TodoItemDeletedEvent(TodoItem item)
    {
        Item = item;
    }

    public TodoItem Item { get; }
}
