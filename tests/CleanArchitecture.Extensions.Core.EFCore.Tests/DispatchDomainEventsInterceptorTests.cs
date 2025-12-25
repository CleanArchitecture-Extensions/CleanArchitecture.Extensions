using CleanArchitecture.Extensions.Core.DomainEvents;
using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Core.EFCore.Tests;

/// <summary>
/// Tests covering the EF Core interceptor that dispatches domain events.
/// </summary>
public class DispatchDomainEventsInterceptorTests
{
    [Fact]
    public async Task Interceptor_AddsCorrelationAndDispatches()
    {
        var tracker = new DomainEventTracker();
        var dispatcher = new RecordingDispatcher();
        var logContext = new InMemoryLogContext();
        var options = Options.Create(new CoreExtensionsOptions { CorrelationIdFactory = () => "cid-interceptor" });
        var interceptor = new DispatchDomainEventsInterceptor(tracker, dispatcher, logContext, options);

        var dbOptions = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        await using (var context = new TestDbContext(dbOptions))
        {
            var aggregate = new TestAggregate();
            aggregate.Trigger();
            context.Aggregates.Add(aggregate);

            await context.SaveChangesAsync();
            Assert.Empty(aggregate.DomainEvents);
        }

        var dispatched = Assert.Single(dispatcher.Events);
        Assert.Equal("cid-interceptor", dispatched.CorrelationId);
        Assert.Equal("cid-interceptor", logContext.CorrelationId);
        Assert.False(tracker.HasEvents);
    }

    [Fact]
    public async Task Interceptor_RespectsExistingEventCorrelation()
    {
        var tracker = new DomainEventTracker();
        var dispatcher = new RecordingDispatcher();
        var logContext = new InMemoryLogContext { CorrelationId = "context-correlation" };
        var options = Options.Create(new CoreExtensionsOptions { CorrelationIdFactory = () => "generated" });
        var interceptor = new DispatchDomainEventsInterceptor(tracker, dispatcher, logContext, options);

        var dbOptions = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        await using var context = new TestDbContext(dbOptions);
        var aggregate = new TestAggregate();
        aggregate.TriggerWithCorrelation("event-correlation");
        context.Aggregates.Add(aggregate);

        await context.SaveChangesAsync();

        var dispatched = Assert.Single(dispatcher.Events);
        Assert.Equal("event-correlation", dispatched.CorrelationId);
        Assert.Equal("context-correlation", logContext.CorrelationId);
    }

    private sealed class RecordingDispatcher : IDomainEventDispatcher
    {
        public List<DomainEvent> Events { get; } = new();

        public Task DispatchAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            Events.Add(domainEvent);
            return Task.CompletedTask;
        }

        public Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            Events.AddRange(domainEvents);
            return Task.CompletedTask;
        }
    }

    private sealed class TestAggregate : AggregateRoot
    {
        public int Id { get; set; }

        public void Trigger() => RaiseDomainEvent(new TestEvent());

        public void TriggerWithCorrelation(string correlationId) => RaiseDomainEvent(new TestEvent(correlationId));
    }

    private sealed record TestEvent(string? CorrelationId = null) : DomainEvent(CorrelationId);

    private sealed class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options)
        {
        }

        public DbSet<TestAggregate> Aggregates => Set<TestAggregate>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestAggregate>().Ignore(aggregate => aggregate.DomainEvents);
            modelBuilder.Ignore<DomainEvent>();
            base.OnModelCreating(modelBuilder);
        }
    }
}
