using MediatR;

namespace CleanArchitecture.Extensions.Core.DomainEvents;

/// <summary>
/// Base type for domain events raised by aggregates.
/// </summary>
public abstract record DomainEvent : INotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEvent"/> class.
    /// </summary>
    /// <param name="correlationId">Optional correlation identifier propagated with the event.</param>
    protected DomainEvent(string? correlationId = null)
    {
        CorrelationId = correlationId;
    }

    /// <summary>
    /// Gets the unique identifier for this event instance.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the UTC timestamp when the event occurred.
    /// </summary>
    public DateTimeOffset OccurredOnUtc { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the correlation identifier that links the event to a request or operation.
    /// </summary>
    public string? CorrelationId { get; init; }
}
