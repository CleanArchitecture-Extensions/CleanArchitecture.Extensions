using CleanArchitecture.Extensions.Core.DomainEvents;

namespace CleanArchitecture.Extensions.Core.Tests;

/// <summary>
/// Tests covering domain event tracking and dispatch.
/// </summary>
public class DomainEventTests
{
    [Fact]
    public void Tracker_Drain_ClearsAndPreservesOrder()
    {
        var tracker = new DomainEventTracker();
        var first = new TestEvent("one");
        var second = new TestEvent("two");

        tracker.Add(first);
        tracker.Add(second);

        var drained = tracker.Drain();

        Assert.Equal(new[] { first, second }, drained);
        Assert.False(tracker.HasEvents);
        Assert.Empty(tracker.Events);
    }

    [Fact]
    public async Task Dispatcher_Dispatches_All_Events()
    {
        var published = new List<DomainEvent>();
        var dispatcher = new CollectingDispatcher(published);
        var events = new[]
        {
            new TestEvent("cid-1"),
            new TestEvent("cid-2")
        };

        await dispatcher.DispatchAsync(events);

        Assert.Equal(2, published.Count);
        Assert.Equal(events.Select(e => e.CorrelationId), published.Select(e => e.CorrelationId));
    }

    private sealed record TestEvent(string? CorrelationId = null) : DomainEvent(CorrelationId);

    private sealed class CollectingDispatcher(List<DomainEvent> published) : IDomainEventDispatcher
    {
        public Task DispatchAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            published.Add(domainEvent);
            return Task.CompletedTask;
        }

        public Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            published.AddRange(domainEvents);
            return Task.CompletedTask;
        }
    }
}
