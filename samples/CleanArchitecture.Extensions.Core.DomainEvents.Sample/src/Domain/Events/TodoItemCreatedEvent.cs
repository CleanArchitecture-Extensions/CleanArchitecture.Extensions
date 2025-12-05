namespace CleanArchitecture.Extensions.Core.DomainEvents.Sample.Domain.Events;

public sealed record TodoItemCreatedEvent(TodoItem Item, string? CorrelationId = null) : BaseEvent(CorrelationId);
