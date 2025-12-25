# Extension: Core

> Deprecated. We are retiring Core and returning to the template primitives. No replacement package is planned.

## Why this package exists

Jason Taylor’s Clean Architecture template ships a thin set of primitives: a minimal `Result` (bool + string array), MediatR behaviors for logging/validation/authorization/performance, domain-event plumbing via EF Core interceptors, Microsoft.Extensions.Logging abstractions, and `TimeProvider` for auditing. Those building blocks are intentionally small so the template stays approachable, but teams often need richer primitives (correlation, error metadata, deterministic time, test-friendly logging) without rewriting the template. The Core extension keeps you aligned with Jason’s wiring while supplying opinionated, dependency-light upgrades:

- Rich `Result`/`Error` types with trace IDs, metadata, and composition helpers.
- Guard clauses that return results, throw, or accumulate errors.
- Pipeline behaviors that preserve the template’s order/signatures while adding correlation scopes and structured logging.
- Logging abstractions (`IAppLogger<T>`, `ILogContext>`) decoupled from any provider, plus in-memory/no-op implementations for tests.
- Domain event base types and a tracker for dispatch pipelines beyond EF interceptors.
- Time abstraction (`IClock`) that wraps `TimeProvider` concepts and unlocks deterministic clocks.

If you want a deep dive into Results specifically, see [Result primitives](./core-result-primitives.md). The rest of this page orients you to everything in the Core package.

## Alignment with the template

- **Result:** In the template (`src/Application/Common/Models/Result.cs`), success is a boolean with `string[] Errors`. It is used primarily by the Identity service (`IdentityService.ToApplicationResult`, `DeleteUserAsync`). There is no correlation metadata or value payload. Core keeps the pattern (success/failure) but adds trace IDs, error codes/messages/metadata, and generic payloads.
- **Pipeline behaviors:** The template wires `LoggingBehaviour` (pre-processor), `UnhandledExceptionBehaviour`, `AuthorizationBehaviour`, `ValidationBehaviour`, and `PerformanceBehaviour` in `DependencyInjection.cs`. Core behaviors mirror these signatures so you can swap without changing registrations, while layering correlation scopes and structured properties.
- **Domain events:** Template entities derive from `BaseEntity` and raise `BaseEvent` (INotification). EF's `DispatchDomainEventsInterceptor` drains events and publishes via MediatR on SaveChanges. Core keeps the MediatR-friendly event shape and supplies a `DomainEventTracker` plus `IDomainEventDispatcher` abstraction for alternate dispatch pipelines (e.g., outbox, bus). The EF Core interceptor lives in `CleanArchitecture.Extensions.Core.EFCore`.
- **Time:** Template uses `TimeProvider` inside `AuditableEntityInterceptor` to stamp `Created`/`LastModified`. Core’s `IClock` wraps similar capabilities (`UtcNow`, `Today`, `Timestamp`, `Delay`, `NewGuid`) with test clocks and offsets.
- **Logging:** Template relies on `ILogger<T>`, `IUser`, and `IIdentityService` to enrich logs. Core introduces provider-agnostic logging + context abstractions so you can plug in Serilog, MEL, or in-memory loggers without coupling application code to a specific provider.

## Package contents (what you get)

### Result primitives (summary)

- `Result` and `Result<T>` capture success/failure, `TraceId`, and `IReadOnlyList<Error>`.
- `Error` holds `Code`, `Message`, optional `Metadata`, and helpers to attach trace IDs.
- Composition helpers: `Map`, `Bind`, `Tap`, `Ensure`, `Recover`, `Combine`, plus convenience conversions (`ToResult`).
- All primitives live in `CleanArchitecture.Extensions.Core.Results` and avoid external dependencies.
- Deep dive: [Result primitives](./core-result-primitives.md).

### Guard clauses

- `Guard` static helpers: null/whitespace, enum, range, min/max length, boolean `Ensure`.
- `GuardOptions` lets you pick a strategy per call: `ReturnFailure` (default), `Throw`, or `Accumulate` (push into an `ErrorSink` collection).
- Trace IDs propagate from options so guard failures stay correlated with the request.
- `GuardStrategy` enum documents strategies and can be derived from shared `CoreExtensionsOptions`.
- Deep dive: [Core Guard Clauses](./core-guard-clauses.md).

### Pipeline behaviors

- **CorrelationBehavior:** Ensures a correlation ID exists (uses `CoreExtensionsOptions.CorrelationIdFactory` or clock-based GUID), pushes it to `ILogContext`, and preserves scope for downstream logs.
- **Logging:** `LoggingPreProcessor<TRequest>` logs start-of-request; `LoggingBehavior<TRequest, TResponse>` logs handling/handled with correlation and request type.
- **PerformanceBehavior:** Measures elapsed time via `IClock`, emits warnings when `PerformanceWarningThreshold` is exceeded, can be toggled with `EnablePerformanceLogging`.
- Ordering guidance (matching the template’s intent): `Correlation` → `Logging (pre)` → `UnhandledException` → `Authorization` → `Validation` → `Performance` → Handler. Insert Core behaviors accordingly to maintain compatibility.
- Deep dive: [Core Pipeline Behaviors](./core-pipeline-behaviors.md).

### Logging abstractions

- `IAppLogger<T>` mirrors `ILogger<T>` severity levels but keeps the surface minimal; overloads for Trace/Debug/Info/Warn/Error/Critical delegate to `Log`.
- `ILogContext` stores `CorrelationId` and supports `PushProperty` to enrich structured logs. You can wrap `ILogger.BeginScope` or Serilog’s `LogContext` in your adapter.
- Implementations: `NoOpAppLogger<T>` for silent runs, `InMemoryAppLogger<T>` + `InMemoryLogContext` for tests/diagnostics, `NoOpLogContext` as a stand-in when scope handling is optional.
- Deep dive: [Core Logging Abstractions](./core-logging-abstractions.md).

### Domain events

- `DomainEvent` base record (INotification) with `Id`, `OccurredOnUtc`, and optional `CorrelationId`.
- `DomainEventTracker` buffers events until dispatched; `Drain` returns a snapshot and clears, mirroring EF interceptor behavior while enabling non-EF dispatch paths.
- `IDomainEventDispatcher` abstraction so Infrastructure can publish to MediatR, a message bus, or an outbox without changing Application.
- Deep dive: [Core Domain Events](./core-domain-events.md).

### Time

- `IClock` abstraction with `UtcNow`, `Today`, `Timestamp`, `NewGuid`, and async `Delay`.
- Implementations: `SystemClock` (live), `FrozenClock` (manually advance for tests), `OffsetClock` (apply fixed offset to an inner clock). Aligns with `TimeProvider` semantics but adds GUID generation for correlation and deterministic tests.
- Deep dive: [Core Time](./core-time.md).

### Options

- `CoreExtensionsOptions` gathers cross-cutting defaults: correlation header name, guard strategy, performance logging toggle/threshold, correlation ID factory, and a default `TraceId` to flow into guards/results.
- `GuardOptions.FromOptions` lets you hydrate guard config from shared options while providing per-call sinks.
- Deep dive: [Core Options](./core-options.md).

## Install

```bash
dotnet add src/YourProject/YourProject.csproj package CleanArchitecture.Extensions.Core
```
If you need the EF Core domain-event interceptor, also install:
```bash
dotnet add src/YourProject/YourProject.csproj package CleanArchitecture.Extensions.Core.EFCore
```

## Integration guide (Application layer)

1. **Register Core + options in one call:**

```csharp
services.AddCleanArchitectureCore(options =>
{
    options.CorrelationHeaderName = "X-Correlation-ID";
    options.GuardStrategy = GuardStrategy.ReturnFailure;
    options.EnablePerformanceLogging = true;
    options.PerformanceWarningThreshold = TimeSpan.FromMilliseconds(500);
});
```

2. **Wire pipeline behaviors with the helper (keep template order):**

```csharp
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddCleanArchitectureCorePipeline(); // Correlation -> Logging pre/post -> Performance
    cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
    cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
});
```

3. **Adopt Results + guards in handlers:** return `Result<T>` from commands/queries; compose with guards and validation.

```csharp
var nameResult = Guard.AgainstNullOrWhiteSpace(request.Name, nameof(request.Name),
    GuardOptions.FromOptions(options.Value));
if (nameResult.IsFailure) return Result.Failure<string>(nameResult.Errors, nameResult.TraceId);

var created = await _repository.CreateAsync(nameResult.Value, cancellationToken);
return Result.Success(created.Id, nameResult.TraceId);
```

## Compatibility notes

- **Drop-in for template behaviors:** Signatures are compatible with `cfg.AddOpenBehavior` and `AddOpenRequestPreProcessor` in the template DI (`LoggingPreProcessor` for pre-processing, `LoggingBehavior` for pipeline). Swap registrations without changing handlers.
- **Interop with existing Result:** You can map template `Result` to/from Core’s `Result` by projecting errors into `Error` codes/messages and vice versa (e.g., `Result.Success().Errors` → `Result.Success(traceId)`).
- **EF interceptors:** `DispatchDomainEventsInterceptor` ships in `CleanArchitecture.Extensions.Core.EFCore`. `DomainEventTracker` can complement the interceptor when you need to buffer events outside DbContext lifetime or forward to a bus/outbox.
- **Time/Auditing:** Replace `TimeProvider` injections with `IClock` adapters that internally call `TimeProvider.System` if you need 1:1 behavior.

## Configuration reference

Configure inline (as above) for clarity in samples. If you prefer configuration binding, call `services.AddCleanArchitectureCore(opts => configuration.GetSection("Extensions:Core").Bind(opts));`.

## Troubleshooting & adoption tips

- Missing correlation ID in logs: ensure `ILogContext` is scoped, `CorrelationBehavior` runs before logging, and your logger adapter copies `ILogContext.CorrelationId` into scopes/enrichers.
- Performance logs silent: confirm `EnablePerformanceLogging` is true and behavior is registered; ensure `IClock` returns consistent `UtcNow`.
- Guard exceptions unexpected: switch strategy to `ReturnFailure` or `Accumulate`; when throwing, set `ExceptionFactory` to shape domain-specific exceptions.
- Too many `Result`-wrapped signatures: keep simple commands/queries returning primitives until you need rich errors; adopt Core Results incrementally via adapters.

## What to read next

- Deep dive on results, composition patterns, and real-world handlers: [Result primitives](./core-result-primitives.md).
- For validation-focused behaviors and FluentValidation hand-offs, see [Validation extension](./validation.md).
- Multitenancy-specific pipeline glue lives in [Multitenancy Core](./multitenancy-core.md); it composes with the Core behaviors described here.
