# Core Domain Events

> Deprecated. This content is retained for reference only.

Domain events in Jason Taylor's Clean Architecture template are simple: `BaseEvent` is an empty `INotification`, `BaseEntity` holds a list of events, and an EF Core `DispatchDomainEventsInterceptor` publishes them via MediatR on `SaveChanges`. The Core extension adds richer metadata, a tracker, and an abstraction for dispatch so you can route events beyond EF, attach correlation IDs, and test event flows without EF or MediatR plumbing. The EF Core interceptor now ships in `CleanArchitecture.Extensions.Core.EFCore` so Domain can consume Core without pulling EF packages. This guide explains the template baseline, what Core adds, how to wire it, and real-world patterns for handlers, integration, and testing.

## What the template already covers
- **Types:** `BaseEvent : INotification` (no properties) and `BaseEntity` keeps `List<BaseEvent> DomainEvents`.
- **Dispatch:** `DispatchDomainEventsInterceptor` (SaveChanges interceptor) drains `DomainEvents` from tracked entities and publishes each via `IMediator.Publish`.
- **Usage:** Entities call `AddDomainEvent(new TodoItemCreatedEvent(...))`. When EF `SaveChanges` runs, events are published in-process to notification handlers.
- **Gaps:** No correlation metadata, no timestamps, no tracking outside EF, and no abstraction to swap the dispatcher (e.g., to an outbox or message bus).

## What Core adds
- **Event base:** `DomainEvent` (record, INotification) with `Id`, `OccurredOnUtc`, and optional `CorrelationId`. `OccurredOnUtc` is driven by the registered `IClock`, so using `FrozenClock` in tests gives deterministic timestamps.
- **Tracking:** `DomainEventTracker` to collect/clear events independent of EF change tracking; useful for aggregates not persisted via EF, or for outbox patterns.
- **Dispatch abstraction:** `IDomainEventDispatcher` defines `DispatchAsync(DomainEvent)` and `DispatchAsync(IEnumerable<DomainEvent>)`, letting Infrastructure choose how to publish (MediatR, bus, outbox).
- **Correlation-first:** Events can carry `CorrelationId` so you can tie logs, results, and integration messages together.
- **Testability:** In-memory tracker/dispatcher implementations make it easy to assert event emission without a database.

## API surface
Namespace: `CleanArchitecture.Extensions.Core.DomainEvents`

- `abstract record DomainEvent(string? correlationId = null)` : `Guid Id`, `DateTimeOffset OccurredOnUtc`, `string? CorrelationId`.
- `DomainEventTracker` : `Add(DomainEvent)`, `HasEvents`, `Drain()`, `Clear()`, `Events` (read-only snapshot).
- `IDomainEventDispatcher` : `DispatchAsync(DomainEvent, CancellationToken)`, `DispatchAsync(IEnumerable<DomainEvent>, CancellationToken)`.
- EF Core adapter: `DispatchDomainEventsInterceptor` (in `CleanArchitecture.Extensions.Core.EFCore`).

## Why use the Core approach
- **Correlation:** Attach `CorrelationId` from `ILogContext` or incoming HTTP headers so downstream handlers/logs share the same ID.
- **Flexibility:** Use the tracker in non-EF scenarios (Dapper, document DBs, in-memory aggregates) and dispatch via any mechanism (MediatR, message bus, outbox table).
- **Testing:** Assert on tracker contents without spinning up EF. Fake dispatchers keep tests fast and deterministic.
- **Consistency:** Aligns with Core logging and Result primitives—errors and events can share the same correlation token for observability.

## Wiring in DI (typical setups)
Install `CleanArchitecture.Extensions.Core.EFCore` when you need the EF Core interceptor.
MediatR in-process dispatch + EF interceptor:
```csharp
builder.Services.AddCleanArchitectureCore(); // registers DomainEventTracker + MediatRDomainEventDispatcher
builder.Services.AddCleanArchitectureCoreEfCore(); // registers EF Core interceptor

builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    options.AddInterceptors(sp.GetRequiredService<DispatchDomainEventsInterceptor>());
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    // pipeline behaviors go here
});
```
- `DispatchDomainEventsInterceptor` (shipped in `CleanArchitecture.Extensions.Core.EFCore`) drains `IHasDomainEvents` aggregates from the EF change tracker, correlates events via `ILogContext`/`CoreExtensionsOptions`, and publishes through `IDomainEventDispatcher`.
- If you use an outbox or bus, replace `IDomainEventDispatcher` with your implementation; the interceptor still gathers and correlates events before dispatch.

## Sample-backed walkthrough (domain events sample)
A runnable solution lives at `samples/CleanArchitecture.Extensions.Core.DomainEvents.Sample`.

### Tracker + dispatcher with correlation
`samples/CleanArchitecture.Extensions.Core.DomainEvents.Sample/src/Infrastructure/Data/Interceptors/DispatchDomainEventsInterceptor.cs`:
```csharp
var correlationId = EnsureCorrelationId();
var correlatedEvents = _tracker
    .Drain()
    .Select(domainEvent => EnsureCorrelation(domainEvent, correlationId))
    .ToArray();

await _dispatcher.DispatchAsync(correlatedEvents, cancellationToken);
```
- Domain events raised in aggregates are captured by `DomainEventTracker`, correlated (using `ILogContext` or the options factory), and dispatched through the `IDomainEventDispatcher`.
- Events without a correlation ID are cloned via record `with` expressions so handlers/logs see the request-scoped identifier.

### Recording dispatched events for diagnostics
`samples/CleanArchitecture.Extensions.Core.DomainEvents.Sample/src/Application/Diagnostics/Queries/GetRecentDomainEvents/GetRecentDomainEventsQuery.cs`:
```csharp
public sealed record GetRecentDomainEventsQuery(int Count = 20) : IRequest<IReadOnlyCollection<DomainEventLogEntry>>;

public Task<IReadOnlyCollection<DomainEventLogEntry>> Handle(GetRecentDomainEventsQuery request, CancellationToken cancellationToken)
{
    var take = Math.Clamp(request.Count, 1, 50);
    var entries = _domainEventLog.ReadRecent(take);
    return Task.FromResult(entries);
}
```
- The sample decorates the dispatcher with an in-memory `IDomainEventLog` and exposes recent events at `/api/DomainEvents/recent`.

### Creating events with request correlation
`samples/CleanArchitecture.Extensions.Core.DomainEvents.Sample/src/Application/TodoItems/Commands/CreateTodoItem/CreateTodoItem.cs`:
```csharp
var correlationId = _logContext.CorrelationId ?? Guid.NewGuid().ToString("N");
_logContext.CorrelationId = correlationId;

entity.AddDomainEvent(new TodoItemCreatedEvent(entity, correlationId));
```
- Correlation is set by middleware (or generated in the handler) and flows into domain events, making it easy to trace end-to-end alongside the recorded event log entries.

## Real-world scenarios

### 1) Correlation-aware events from handlers
```csharp
public sealed class CreateOrderCommandHandler : IRequest<Result<Guid>>
{
    private readonly IOrderRepository _orders;
    private readonly IDomainEventDispatcher _dispatcher;
    private readonly ILogContext _logContext;

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var correlationId = _logContext.CorrelationId ?? Guid.NewGuid().ToString("N");
        var order = Order.Create(request.CustomerId, correlationId); // domain factory sets correlation on events

        await _orders.AddAsync(order, ct);
        await _dispatcher.DispatchAsync(order.DomainEvents, ct);
        order.ClearDomainEvents();

        return Result.Success(order.Id, correlationId);
    }
}
```
- Events and Result share the same correlation ID, making logs and downstream handlers traceable.

### 2) Outbox-friendly dispatch
```csharp
public sealed class OutboxDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IOutboxWriter _outbox;

    public Task DispatchAsync(DomainEvent domainEvent, CancellationToken ct = default) =>
        _outbox.AppendAsync(new OutboxMessage(domainEvent), ct);

    public Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken ct = default) =>
        _outbox.AppendAsync(domainEvents.Select(e => new OutboxMessage(e)), ct);
}
```
- Swap dispatchers per environment: in-process for dev/tests; outbox for production.

### 3) Aggregating events outside EF
```csharp
public sealed class InMemoryAggregateStore
{
    private readonly DomainEventTracker _tracker;

    public InMemoryAggregateStore(DomainEventTracker tracker) => _tracker = tracker;

    public void Save(AggregateRoot aggregate)
    {
        foreach (var e in aggregate.DomainEvents)
        {
            _tracker.Add(e);
        }
        aggregate.ClearDomainEvents();
    }
}
```
- Later, call `_dispatcher.DispatchAsync(_tracker.Drain())` to publish.

### 4) Mapping existing template events
If you have existing `BaseEvent` types from the template, wrap or replace them:
```csharp
public sealed record TodoItemCreatedEvent(Guid ItemId, string? CorrelationId = null) 
    : DomainEvent(CorrelationId);
```
- You can keep handler code; MediatR sees it as `INotification` like before.

### 5) Logging event publication
```csharp
public sealed class LoggingDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IDomainEventDispatcher _inner;
    private readonly IAppLogger<LoggingDomainEventDispatcher> _logger;
    private readonly ILogContext _context;

    public LoggingDomainEventDispatcher(IDomainEventDispatcher inner, IAppLogger<LoggingDomainEventDispatcher> logger, ILogContext context)
    {
        _inner = inner;
        _logger = logger;
        _context = context;
    }

    public async Task DispatchAsync(DomainEvent domainEvent, CancellationToken ct = default)
    {
        _logger.Info("Dispatching domain event", new Dictionary<string, object?>
        {
            ["EventType"] = domainEvent.GetType().Name,
            ["EventId"] = domainEvent.Id,
            ["CorrelationId"] = domainEvent.CorrelationId ?? _context.CorrelationId
        });
        await _inner.DispatchAsync(domainEvent, ct);
    }

    public async Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken ct = default)
    {
        foreach (var e in domainEvents)
        {
            await DispatchAsync(e, ct);
        }
    }
}
```
- Decorators let you log without changing handler or dispatcher implementations.

## Patterns and guidance
- **Set correlation at creation time:** Pass `ILogContext.CorrelationId` into domain event constructors in factories/handlers so it flows everywhere.
- **Drain after dispatch:** When using trackers, call `Drain()` to get a snapshot and clear them, mirroring the EF interceptor behavior.
- **Avoid double dispatch:** If you keep the template interceptor, ensure you’re not also manually dispatching the same events. Choose one dispatch path per unit of work.
- **Event ordering:** `DomainEventTracker` preserves insertion order. If ordering matters across aggregates, consider sequencing or timestamp-based ordering in your dispatcher.
- **Exception handling:** Decide whether dispatcher exceptions should fail the transaction (typical) or be retried via outbox. The abstraction lets you choose.
- **Background jobs:** When handling commands in background workers, you can still use the same behaviors; ensure a correlation ID is set (middleware or behavior).

## Testing domain events
Using the tracker + mediator dispatcher:
```csharp
[Fact]
public async Task DispatchAsync_Publishes_All_Tracked_Events()
{
    var tracker = new DomainEventTracker();
    var published = new List<DomainEvent>();
    var dispatcher = new StubDispatcher(published);
    tracker.Add(new TestEvent("corr-1"));
    tracker.Add(new TestEvent("corr-1"));

    await dispatcher.DispatchAsync(tracker.Drain());

    published.Should().HaveCount(2);
    published.All(e => e.CorrelationId == "corr-1").Should().BeTrue();
}

file sealed class TestEvent(string? CorrelationId = null) : DomainEvent(CorrelationId);
file sealed class StubDispatcher(List<DomainEvent> published) : IDomainEventDispatcher
{
    public Task DispatchAsync(DomainEvent domainEvent, CancellationToken ct = default)
    {
        published.Add(domainEvent);
        return Task.CompletedTask;
    }

    public Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken ct = default)
    {
        published.AddRange(domainEvents);
        return Task.CompletedTask;
    }
}
```

## Integration with Results and logging
- Use the same correlation ID across Results, logs, and events:
  - `var correlationId = _logContext.CorrelationId ?? Guid.NewGuid().ToString("N");`
  - Pass to domain events: `new OrderSubmittedEvent(orderId, correlationId)`.
  - Return `Result.Success(orderId, correlationId)`.
- Logging adapters can add `CorrelationId` to scopes; event dispatchers can log using the same ID, making troubleshooting and tracing straightforward.

## Migration from template
- Replace `BaseEvent` with Core `DomainEvent` to get correlation and timestamps. Existing handlers still work because it’s an `INotification`.
- Optionally replace `DispatchDomainEventsInterceptor` with a version that uses `DomainEventTracker` + `IDomainEventDispatcher` if you need more control (outbox, bus).
- Keep `BaseEntity` domain event list concept; ensure your entities expose/clear events just like the template.
- You can mix old and new event types during migration; MediatR can handle both.

## FAQ
- **Do I need a tracker if I use EF interceptor?** Not strictly. The interceptor can dispatch directly. Use the tracker when you want to buffer events, inspect them, or reuse in non-EF contexts.
- **Can I publish to a message bus?** Yes—implement `IDomainEventDispatcher` to translate events to bus messages. Use correlation IDs for tracing across services.
- **Should events carry state or IDs?** Prefer IDs and minimal snapshots. Use handlers to load additional data if needed. Keep payloads lean to avoid duplication and staleness.
- **How do I avoid duplicate publishes?** Ensure each event is cleared after dispatch. When using outbox, let the outbox process ensure exactly-once semantics for external delivery.
- **What about transactions?** For in-process MediatR handlers, dispatch before commit to keep consistency, or use outbox to decouple. The abstraction lets you choose per use case.

## Adoption checklist
1) Introduce `DomainEvent` for new events; add `CorrelationId` when available from `ILogContext`.
2) Register `IDomainEventDispatcher` (mediator, outbox, or bus) and optionally `DomainEventTracker`.
3) If using EF, wire an interceptor (or replace the template interceptor) that collects events and dispatches via the dispatcher.
4) Standardize event naming and payloads; keep them immutable and small.
5) Update tests to assert on `DomainEventTracker` contents or dispatcher invocations.
6) Log dispatches (via decorator) if you need observability of event publication paths.

## Related docs
- [Core pipeline behaviors](./core-pipeline-behaviors.md) for correlation/logging that feeds correlation IDs into events.
- [Core logging abstractions](./core-logging-abstractions.md) for scopes/adapters used by dispatchers.
- [Core result primitives](./core-result-primitives.md) to align correlation IDs across results and events.
- [Core guard clauses](./core-guard-clauses.md) for input validation that may precede event emission.
- [Core extension overview](./core.md) for the package-level summary and registration guidance.
