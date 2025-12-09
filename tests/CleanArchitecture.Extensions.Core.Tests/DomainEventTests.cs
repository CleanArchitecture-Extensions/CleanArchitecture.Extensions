using CleanArchitecture.Extensions.Core.DomainEvents;
using System.Linq;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using CleanArchitecture.Extensions.Core.Time;
using CleanArchitecture.Extensions.Core.Options;

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

    [Fact]
    public void AggregateRoot_RaisesAndClears_Events()
    {
        var aggregate = new TestAggregate();
        aggregate.DoSomething();

        Assert.Single(aggregate.DomainEvents);

        var drained = aggregate.DequeueDomainEvents();
        Assert.Single(drained);
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public async Task DispatchAndClear_Dispatches_From_Aggregates()
    {
        var aggregate = new TestAggregate();
        aggregate.DoSomething();
        var published = new List<DomainEvent>();
        var dispatcher = new CollectingDispatcher(published);

        await dispatcher.DispatchAndClearAsync(new[] { aggregate });

        Assert.Single(published);
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public async Task MediatRDispatcher_Publishes_Notifications()
    {
        var mediator = new CollectingMediator();
        var dispatcher = new MediatRDomainEventDispatcher(mediator);
        var evt = new TestEvent("cid-mediatr");

        await dispatcher.DispatchAsync(evt);
        await dispatcher.DispatchAsync(new[] { evt });

        Assert.Equal(2, mediator.Published.Count);
        Assert.All(mediator.Published, e => Assert.Equal("cid-mediatr", e.CorrelationId));
    }

    [Fact]
    public void DomainEventTime_UsesConfiguredClock_WhenOptionsMaterialized()
    {
        var services = new ServiceCollection();
        var frozen = new FrozenClock(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero));
        services.AddSingleton<IClock>(frozen);
        services.AddCleanArchitectureCore();

        // Materialize options to trigger post-configure hook.
        using var provider = services.BuildServiceProvider();
        _ = provider.GetRequiredService<IOptions<CoreExtensionsOptions>>().Value;

        var before = DomainEventTime.Now;
        frozen.Advance(TimeSpan.FromMinutes(5));
        var after = DomainEventTime.Now;

        Assert.Equal(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero), before);
        Assert.Equal(new DateTimeOffset(2030, 1, 1, 0, 5, 0, TimeSpan.Zero), after);

        // Reset to avoid leaking clock into other tests.
        DomainEventTime.SetProvider(() => DateTimeOffset.UtcNow);
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

    private sealed class TestAggregate : AggregateRoot
    {
        public void DoSomething() => RaiseDomainEvent(new TestEvent("agg"));
    }

    private sealed class CollectingMediator : IMediator
    {
        public List<DomainEvent> Published { get; } = new();

        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            if (notification is DomainEvent domainEvent)
            {
                Published.Add(domainEvent);
            }

            return Task.CompletedTask;
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            if (notification is DomainEvent domainEvent)
            {
                Published.Add(domainEvent);
            }

            return Task.CompletedTask;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            Task.FromResult(default(TResponse)!);

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
            Task.FromResult(default(object));

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest =>
            Task.CompletedTask;

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            Empty<TResponse>();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
            Empty<object?>();

        private static async IAsyncEnumerable<T> Empty<T>()
        {
            yield break;
        }
    }
}
