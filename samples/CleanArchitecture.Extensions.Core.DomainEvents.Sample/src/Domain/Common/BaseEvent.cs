using CleanArchitecture.Extensions.Core.DomainEvents;

namespace CleanArchitecture.Extensions.Core.DomainEvents.Sample.Domain.Common;

public abstract record BaseEvent(string? CorrelationId = null) : DomainEvent(CorrelationId);
