# Core Time Abstractions

Time is a cross-cutting concern in Jason Taylor’s Clean Architecture template. The template uses `TimeProvider` inside the EF Core `AuditableEntityInterceptor` to stamp `Created`/`LastModified` and uses `Stopwatch` in `PerformanceBehaviour` for timing. The Core extension introduces a unified `IClock` abstraction with system, frozen, and offset implementations, plus GUID generation and async delay, so you can keep tests deterministic, inject time consistently across layers, and align correlation/timing with other Core primitives.

## What the template already covers
- **Auditing:** `AuditableEntityInterceptor` injects `TimeProvider` to set `Created`/`LastModified` timestamps in `BaseAuditableEntity`.
- **Performance:** `PerformanceBehaviour` (template) uses a `Stopwatch` directly to time handlers; no configuration or correlation metadata is attached.
- **No clock abstraction:** Outside of the interceptor, handlers often call `DateTime.UtcNow` directly. There’s no interface to mock or offset time across the app.

## What Core adds
- **`IClock` interface:** Provides `UtcNow`, `Today`, `Timestamp`, `NewGuid()`, and async `Delay(...)`.
- **Implementations:** 
  - `SystemClock` wraps system time + `Stopwatch` and GUID generation.
  - `FrozenClock` for deterministic tests; time advances only when you call `Advance`.
  - `OffsetClock` applies a fixed offset to an inner clock (simulate time zones or “time travel” for testing).
- **Consistency:** Behaviors (logging/performance), guards, results, and correlation flows can share the same clock, improving determinism and observability.
- **Testability:** No more scattered `DateTime.UtcNow`; inject `IClock` and use `FrozenClock` in tests to avoid flakiness.

## API surface
Namespace: `CleanArchitecture.Extensions.Core.Time`

- `IClock`
  - `DateTimeOffset UtcNow { get; }`
  - `DateOnly Today { get; }`
  - `long Timestamp { get; }`
  - `Guid NewGuid();`
  - `Task Delay(TimeSpan delay, CancellationToken cancellationToken = default);`
- Implementations:
  - `SystemClock`
  - `FrozenClock`
  - `OffsetClock`

## Why this matters for Clean Architecture
- **Layer decoupling:** Application/Domain code depends on `IClock`, not `DateTime.UtcNow` or `TimeProvider`. Infrastructure can wrap `TimeProvider.System` or other sources as needed.
- **Deterministic tests:** Use `FrozenClock` to freeze time and advance manually; assert timestamps, elapsed durations, and GUID flows without relying on real time.
- **Correlation alignment:** Behaviors and handlers can share `IClock.NewGuid()` for correlation IDs if desired, keeping randomness under one abstraction.
- **Simulation:** `OffsetClock` helps simulate different time zones or daylight savings impacts without changing system time.

## Wiring in DI
```csharp
services.AddSingleton<IClock, SystemClock>(); // default
```
For tests, override with `FrozenClock` or `OffsetClock` via DI configuration:
```csharp
services.AddSingleton<IClock>(_ => new FrozenClock(DateTimeOffset.Parse("2025-01-01T00:00:00Z")));
```

## Sample-backed walkthrough (time sample)
A runnable solution lives at `samples/CleanArchitecture.Extensions.Core.Time.Sample`.

### Snapshot current time with `IClock`
`samples/CleanArchitecture.Extensions.Core.Time.Sample/src/Application/Diagnostics/Queries/GetTimeSnapshot/GetTimeSnapshotQuery.cs`:
```csharp
var guid = _clock.NewGuid();
var now = _clock.UtcNow;
var today = _clock.Today;

var snapshot = new TimeSnapshotDto(now, today, _clock.Timestamp, guid, null);
```
- Exposed via `GET /api/Diagnostics/time` to show `UtcNow`, `Today`, a timestamp, and a GUID sourced from the clock.

### Delay without sleeping in tests
`samples/CleanArchitecture.Extensions.Core.Time.Sample/src/Application/Diagnostics/Commands/SimulateDelay/SimulateDelayCommand.cs`:
```csharp
var started = _clock.UtcNow;
await _clock.Delay(delay, cancellationToken);
var ended = _clock.UtcNow;
var observed = ended - started;
```
- `POST /api/Diagnostics/delay` echoes requested/observed delays; with `SystemClock` it really waits, while tests swap in `FrozenClock` so time advances instantly.

### Deterministic tests with `FrozenClock`
`samples/CleanArchitecture.Extensions.Core.Time.Sample/tests/Application.UnitTests/Diagnostics/TimeDiagnosticsTests.cs`:
```csharp
var clock = new FrozenClock(fixedTime);
var handler = new GetTimeSnapshotQueryHandler(clock);
var result = await handler.Handle(new GetTimeSnapshotQuery(), CancellationToken.None);

result.UtcNow.ShouldBe(fixedTime);
result.EndedAtUtc.ShouldBe(fixedTime.AddMilliseconds(250)); // after simulated delay
```
- Demonstrates advancing `FrozenClock` via `Delay` without real waits, keeping handler assertions deterministic.

## Adapting the template’s auditing interceptor
You can keep the existing `AuditableEntityInterceptor` but inject `IClock` to stay consistent:
```csharp
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly IUser _user;
    private readonly IClock _clock;

    public AuditableEntityInterceptor(IUser user, IClock clock)
    {
        _user = user;
        _clock = clock;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                var utcNow = _clock.UtcNow;
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = _user.Id;
                    entry.Entity.Created = utcNow;
                }
                entry.Entity.LastModifiedBy = _user.Id;
                entry.Entity.LastModified = utcNow;
            }
        }
    }
}
```
- Swap `TimeProvider` for `IClock` to keep auditing consistent with the rest of Core.

## Using `IClock` in pipeline behaviors
Core behaviors already depend on `IClock`:
- `LoggingBehavior` uses `UtcNow` to timestamp start logs and `NewGuid` to seed correlation when missing.
- `PerformanceBehavior` uses `UtcNow` to measure elapsed; configurable thresholds live in `CoreExtensionsOptions`.
- This ensures performance logs and correlation IDs can be deterministic in tests (swap in `FrozenClock`).

## Patterns and examples

### 1) Deterministic tests with FrozenClock
```csharp
[Fact]
public async Task Delay_Advances_FrozenClock()
{
    var clock = new FrozenClock(DateTimeOffset.Parse("2025-01-01T00:00:00Z"));
    await clock.Delay(TimeSpan.FromSeconds(5));
    clock.UtcNow.Should().Be(DateTimeOffset.Parse("2025-01-01T00:00:05Z"));
    clock.Timestamp.Should().Be(TimeSpan.FromSeconds(5).Ticks);
}
```
- `Delay` advances time immediately without sleeping; great for timeout/retry logic tests.

### 2) Time zone simulation with OffsetClock
```csharp
var utcClock = new SystemClock();
var estClock = new OffsetClock(utcClock, TimeSpan.FromHours(-5));
var estNow = estClock.UtcNow; // effectively UTC-5 view
```
- Useful when you need to display or test region-specific dates without altering system time.

### 3) Injecting clock into domain services
```csharp
public sealed class TokenService
{
    private readonly IClock _clock;
    public TokenService(IClock clock) => _clock = clock;

    public TokenIssueResult Issue(string subject)
    {
        var issuedAt = _clock.UtcNow;
        var expires = issuedAt.AddMinutes(30);
        var token = _clock.NewGuid().ToString("N");
        return new TokenIssueResult(token, issuedAt, expires);
    }
}
```
- No static `DateTime.UtcNow`; tests can assert exact timestamps with `FrozenClock`.

### 4) Combining with Results for expiration
```csharp
public Result ValidateNotExpired(DateTimeOffset expiresAt, IClock clock)
{
    return clock.UtcNow <= expiresAt
        ? Result.Success()
        : Result.Failure(new Error("token.expired", "Token is expired", traceId: null));
}
```
- Using the clock keeps logic testable and consistent with behaviors.

### 5) Using Timestamp for perf without Stopwatch
If you prefer high-resolution ticks:
```csharp
var start = _clock.Timestamp;
// do work
var elapsedTicks = _clock.Timestamp - start;
var elapsedMs = elapsedTicks * 1000.0 / Stopwatch.Frequency;
```
- `SystemClock.Timestamp` delegates to `Stopwatch.GetTimestamp`; tests can override with `FrozenClock` if you set `_timestamp` appropriately when advancing.

## Interop and migration tips
- **From `DateTime.UtcNow`:** Inject `IClock` where you need time. For minimal code churn, add a constructor parameter and assign to a private field; replace calls to `DateTime.UtcNow` with `_clock.UtcNow`.
- **From `TimeProvider`:** Create an adapter that implements `IClock` by delegating to `TimeProvider`:
```csharp
public sealed class TimeProviderClock : IClock
{
    private readonly TimeProvider _provider;
    public TimeProviderClock(TimeProvider provider) => _provider = provider;

    public DateTimeOffset UtcNow => _provider.GetUtcNow();
    public DateOnly Today => DateOnly.FromDateTime(_provider.GetUtcNow().UtcDateTime);
    public long Timestamp => _provider.GetTimestamp();
    public Guid NewGuid() => Guid.NewGuid();
    public Task Delay(TimeSpan delay, CancellationToken cancellationToken = default) =>
        _provider.Delay(delay, cancellationToken);
}
```
- Register `TimeProviderClock` in Infrastructure if you rely on `TimeProvider` elsewhere.

## Correlation alignment
- If you want correlation IDs to be GUIDs generated by the same source, use `IClock.NewGuid()` in `CorrelationBehavior` (as Core does by default). This keeps randomness and time abstractions under one roof.
- For deterministic correlation in tests, seed `FrozenClock` and use `NewGuid` from a predictable GUID source if needed (e.g., inject a GUID factory or extend `FrozenClock`).

## Configuration touchpoints
Time itself has no options in `CoreExtensionsOptions`, but it influences behaviors that have options (`PerformanceWarningThreshold`, `EnablePerformanceLogging`, `CorrelationIdFactory`). Use `IClock` consistently to avoid divergence between time and correlation behaviors.

## Testing guidance
- Use `FrozenClock` in unit tests for services/handlers that depend on time.
- Use `OffsetClock` to test date boundary conditions (end-of-day, DST transitions).
- For integration tests of performance logging, pair `FrozenClock` with `InMemoryAppLogger` to assert elapsed times and correlation IDs.

## FAQ
- **Does `FrozenClock` auto-advance?** Only when you call `Advance` or `Delay`. It will not change on its own.
- **Is `OffsetClock` thread-safe?** It delegates to an inner clock; thread safety follows the inner implementation. `SystemClock` is thread-safe; `FrozenClock` uses simple fields and is safe for typical test usage.
- **Should domain entities depend on `IClock`?** Prefer passing timestamps into entities from services/handlers to keep entities pure. Factories can receive `IClock` and pass values to entity constructors.
- **Can I mock `IClock` manually?** Yes—implement a test double or use a mocking framework; the interface is small.

## Adoption checklist
1) Register `IClock` (SystemClock by default) in DI.
2) Swap time usages in Application/Infrastructure to `IClock` (auditing interceptor, handlers, services).
3) For tests, replace registration with `FrozenClock` or `OffsetClock`.
4) Align performance logging and correlation behaviors to use the injected clock (already true for Core behaviors).
5) Consider a `TimeProviderClock` adapter if your Infrastructure already relies on `TimeProvider`.

## Related docs
- [Core pipeline behaviors](./core-pipeline-behaviors.md) for performance/correlation that depend on `IClock`.
- [Core logging abstractions](./core-logging-abstractions.md) where correlation may use `IClock.NewGuid()`.
- [Core result primitives](./core-result-primitives.md) for aligning trace IDs with correlation/time flows.
- [Core guard clauses](./core-guard-clauses.md) and [Core domain events](./core-domain-events.md) to see how time can interplay with validation and event stamping.
- [Core extension overview](./core.md) for the package summary and registration guidance.
