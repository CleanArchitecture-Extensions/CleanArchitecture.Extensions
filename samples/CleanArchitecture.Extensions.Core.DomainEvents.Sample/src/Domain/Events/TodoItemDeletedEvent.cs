namespace CleanArchitecture.Extensions.Core.DomainEvents.Sample.Domain.Events;

public sealed record TodoItemDeletedEvent(TodoItem Item, string? CorrelationId = null) : BaseEvent(CorrelationId);
