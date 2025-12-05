namespace CleanArchitecture.Extensions.Core.DomainEvents.Sample.Domain.Events;

public sealed record TodoItemCompletedEvent(TodoItem Item, string? CorrelationId = null) : BaseEvent(CorrelationId);
