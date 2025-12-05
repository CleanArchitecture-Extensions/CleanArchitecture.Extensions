# Core Logging Abstractions

Logging in Clean Architecture is intentionally minimal in Jason Taylor’s template: it relies on `ILogger<T>` plus user/context services inside MediatR behaviors. The Core extension adds provider-agnostic logging and context abstractions so you can swap providers, capture correlation, and test without mocking concrete logging frameworks. This page explains the template baseline, what the Core abstractions add, how to wire them, and how to adapt them to providers like Microsoft.Extensions.Logging (MEL) or Serilog. It also shows how correlation and Results fit together so logs, errors, and responses share identifiers.

## What the template already covers
- **Behaviors:** `LoggingBehaviour<TRequest>` (pre-processor) uses `ILogger<TRequest>`, `IUser`, and `IIdentityService` to log the request name, user ID, and user name. `PerformanceBehaviour` uses `ILogger<TRequest>` with a `Stopwatch` to warn on slow requests (>500 ms). `UnhandledExceptionBehaviour` logs exceptions with `ILogger<TRequest>`.
- **Context:** There is no dedicated log context abstraction. Correlation is not explicitly propagated; any correlation is left to ambient scopes set by the hosting pipeline.
- **Result + errors:** The template’s `Result` has `Succeeded` and `string[] Errors`; logs do not attach trace IDs or structured error metadata.
- **Time:** Behaviors depend directly on `Stopwatch`/system time, not an abstraction.

This setup is fine for demos, but teams often need:
- Consistent correlation IDs across logs, results, and HTTP responses.
- Provider-agnostic logging contracts so Application layer code and behaviors don’t depend on MEL.
- In-memory/no-op loggers for tests and quiet scenarios.
- Structured properties without coupling to a specific logger’s API.

## What the Core logging abstractions add
- **Interfaces, not providers:** `IAppLogger<T>` mirrors common log levels but stays provider-neutral. `ILogContext` stores correlation ID and exposes `PushProperty` for scoped properties.
- **Implementations for tests/quiet runs:** `InMemoryAppLogger<T>` captures entries with correlation and properties; `NoOpAppLogger<T>` discards logs. `InMemoryLogContext` stores properties in-memory with scoped push/pop; `NoOpLogContext` is a no-op scope holder.
- **Structured entries:** `LogEntry` record captures `Timestamp`, `Level`, `Message`, `CorrelationId`, optional `Exception`, and optional `Properties`.
- **Integration with behaviors:** Core pipeline behaviors depend on these abstractions, letting you plug in your provider with a small adapter.
- **Correlation-first:** `ILogContext.CorrelationId` is the single place for correlation used by behaviors; you can set it from HTTP headers or allow `CorrelationBehavior` to generate it.

## API surface
Namespace: `CleanArchitecture.Extensions.Core.Logging`

- `IAppLogger<T>` with `Log(LogLevel level, string message, Exception? exception = null, IReadOnlyDictionary<string, object?>? properties = null)` plus convenience methods `Trace/Debug/Info/Warn/Error/Critical`.
- `ILogContext` with `string? CorrelationId { get; set; }` and `IDisposable PushProperty(string name, object? value)`.
- Implementations:
  - `InMemoryAppLogger<T>` (stores entries thread-safely).
  - `NoOpAppLogger<T>` (ignores logs).
  - `InMemoryLogContext` (stores correlation ID + properties; supports scoped push/pop).
  - `NoOpLogContext` (does nothing; safe stub).
- Supporting types:
  - `LogEntry` record (timestamp, level, message, correlation ID, exception, properties).
  - `LogLevel` enum (Trace, Debug, Information, Warning, Error, Critical, None).

## Why this matters for Clean Architecture
- **Layer isolation:** Application layer doesn’t depend on MEL/Serilog; Infrastructure can choose the provider and supply adapters. Tests run without heavy logging dependencies.
- **Correlation consistency:** Behaviors ensure correlation IDs reach logs; handlers can propagate the same ID into Results and domain events.
- **Observability readiness:** Structured properties enable search and analytics (e.g., request type, elapsed time, tenant) without forcing a specific logging library.
- **Testability:** In-memory logger/context make assertions straightforward in unit/integration tests.

## Wiring in DI
Minimum registration to get logging working with Core behaviors:
```csharp
services.AddScoped<ILogContext, InMemoryLogContext>();              // replace with provider-specific context
services.AddScoped(typeof(IAppLogger<>), typeof(NoOpAppLogger<>));   // replace with adapter for your logger
services.AddSingleton<IClock, SystemClock>();                       // behaviors need a clock
```
If you’re using Core pipeline behaviors, ensure they’re registered (see the Pipeline Behaviors doc). You can swap `NoOpAppLogger<>` with an adapter that bridges to MEL or Serilog.

## Adapting to Microsoft.Extensions.Logging (MEL)
```csharp
public sealed class MelAppLogger<T> : IAppLogger<T>
{
    private readonly ILogger<T> _logger;
    private readonly ILogContext _context;

    public MelAppLogger(ILogger<T> logger, ILogContext context)
    {
        _logger = logger;
        _context = context;
    }

    public void Log(LogLevel level, string message, Exception? exception = null, IReadOnlyDictionary<string, object?>? properties = null)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = _context.CorrelationId
        });

        _logger.Log(Map(level), exception, message + " {@props}", properties);
    }

    private Microsoft.Extensions.Logging.LogLevel Map(LogLevel level) => level switch
    {
        LogLevel.Trace => Microsoft.Extensions.Logging.LogLevel.Trace,
        LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
        LogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
        LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
        LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
        LogLevel.Critical => Microsoft.Extensions.Logging.LogLevel.Critical,
        _ => Microsoft.Extensions.Logging.LogLevel.None
    };
}
```
Register it:
```csharp
services.AddScoped(typeof(IAppLogger<>), typeof(MelAppLogger<>));
services.AddScoped<ILogContext, InMemoryLogContext>(); // or your own scope wrapper
```

## Adapting to Serilog
```csharp
public sealed class SerilogAppLogger<T> : IAppLogger<T>
{
    private readonly ILogger _logger; // Serilog.ILogger
    private readonly ILogContext _context;

    public SerilogAppLogger(ILogger logger, ILogContext context)
    {
        _logger = logger.ForContext("SourceContext", typeof(T).FullName ?? typeof(T).Name);
        _context = context;
    }

    public void Log(LogLevel level, string message, Exception? exception = null, IReadOnlyDictionary<string, object?>? properties = null)
    {
        var enriched = _logger.ForContext("CorrelationId", _context.CorrelationId);
        if (properties is { Count: > 0 })
        {
            foreach (var kvp in properties)
            {
                enriched = enriched.ForContext(kvp.Key, kvp.Value, destructureObjects: true);
            }
        }

        enriched.Write(Map(level), exception, message);
    }

    private Serilog.Events.LogEventLevel Map(LogLevel level) => level switch
    {
        LogLevel.Trace => Serilog.Events.LogEventLevel.Verbose,
        LogLevel.Debug => Serilog.Events.LogEventLevel.Debug,
        LogLevel.Information => Serilog.Events.LogEventLevel.Information,
        LogLevel.Warning => Serilog.Events.LogEventLevel.Warning,
        LogLevel.Error => Serilog.Events.LogEventLevel.Error,
        LogLevel.Critical => Serilog.Events.LogEventLevel.Fatal,
        _ => Serilog.Events.LogEventLevel.Information
    };
}
```
Register with a Serilog-backed `ILogContext` (could wrap `LogContext.PushProperty` if desired).

## Sample-backed walkthrough (logging sample)
A runnable solution lives at `samples/CleanArchitecture.Extensions.Core.Logging.Sample`.

### Composite logger that records + forwards to MEL
`samples/CleanArchitecture.Extensions.Core.Logging.Sample/src/Application/Common/Logging/CompositeAppLogger.cs`:
```csharp
public sealed class CompositeAppLogger<T> : IAppLogger<T>
{
    public void Log(LogLevel level, string message, Exception? exception = null, IReadOnlyDictionary<string, object?>? properties = null)
    {
        var correlationId = _logContext.CorrelationId;
        using var correlationScope = _logger.BeginScope(new Dictionary<string, object?> { ["CorrelationId"] = correlationId });

        IDisposable? propsScope = null;
        if (properties is { Count: > 0 })
        {
            propsScope = _logger.BeginScope(properties);
        }

        _logger.Log(MapLevel(level), exception, message);
        propsScope?.Dispose();

        _recorder.Record(new LogEntry(
            DateTimeOffset.UtcNow,
            level,
            message,
            correlationId,
            exception,
            properties));
    }
}
```
- Logs flow to `ILogger<T>` scopes with correlation and are also stored in-memory via `ILogRecorder` for diagnostics.

### Correlation middleware + diagnostics endpoints
`samples/CleanArchitecture.Extensions.Core.Logging.Sample/src/Web/Infrastructure/CorrelationMiddleware.cs` ensures every request has a correlation ID and pushes it into `ILogContext`.

`samples/CleanArchitecture.Extensions.Core.Logging.Sample/src/Application/Diagnostics/Commands/EmitLogPulse/EmitLogPulseCommand.cs` shows emitting structured logs with correlation and optional warning/error entries:
```csharp
var correlationId = _logContext.CorrelationId ?? Guid.NewGuid().ToString("N");
_logContext.CorrelationId = correlationId;
using var featureScope = _logContext.PushProperty("Feature", "LoggingSample");

_logger.Log(LogLevel.Information, $"Received log pulse: {request.Message ?? "(no message)"}", properties: properties);
```
`GET /api/Diagnostics/logs` (see `.../Endpoints/Diagnostics.cs`) returns recent `LogEntry` items recorded by the in-memory recorder so you can see correlation + structured properties end-to-end.

## Correlation flows
- `CorrelationBehavior` sets `ILogContext.CorrelationId` early in the pipeline.
- `LoggingBehavior` uses `ILogContext` to push `CorrelationId` into scopes.
- `PerformanceBehavior` includes `CorrelationId` in structured properties.
- Handlers can propagate `_logContext.CorrelationId` into `Result.TraceId` or `DomainEvent.CorrelationId`.
- HTTP middleware can set `ILogContext.CorrelationId` from `CoreExtensionsOptions.CorrelationHeaderName` and echo it back to clients.

Example middleware:
```csharp
public class CorrelationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogContext _context;
    private readonly CoreExtensionsOptions _options;

    public CorrelationMiddleware(RequestDelegate next, ILogContext context, IOptions<CoreExtensionsOptions> options)
    {
        _next = next;
        _context = context;
        _options = options.Value;
    }

    public async Task Invoke(HttpContext http)
    {
        var header = _options.CorrelationHeaderName;
        var incoming = http.Request.Headers[header].FirstOrDefault();
        _context.CorrelationId = string.IsNullOrWhiteSpace(incoming) ? _options.CorrelationIdFactory() : incoming;
        using var scope = _context.PushProperty("CorrelationId", _context.CorrelationId);
        http.Response.Headers[header] = _context.CorrelationId!;
        await _next(http);
    }
}
```

## Using InMemoryAppLogger in tests
```csharp
[Fact]
public async Task LoggingBehavior_WritesStartAndEnd_WithCorrelation()
{
    var clock = new FrozenClock(DateTimeOffset.Parse("2025-01-01T00:00:00Z"));
    var context = new InMemoryLogContext { CorrelationId = "corr-123" };
    var logger = new InMemoryAppLogger<TestRequest>(context);
    var options = Microsoft.Extensions.Options.Options.Create(new CoreExtensionsOptions());
    var behavior = new LoggingBehavior<TestRequest, Unit>(logger, context, clock, options);

    await behavior.Handle(new TestRequest(), _ => Task.FromResult(Unit.Value), CancellationToken.None);

    logger.Entries.Should().Contain(e => e.Message.Contains("Handling") && e.CorrelationId == "corr-123");
    logger.Entries.Should().Contain(e => e.Message.Contains("Handled") && e.CorrelationId == "corr-123");
}
```
- `InMemoryAppLogger` captures correlation automatically from `ILogContext`.

## Handling exceptions and errors
- Behaviors don’t automatically log exceptions; `UnhandledExceptionBehaviour` (template) still does that with `ILogger<T>`. You can adapt it to `IAppLogger<T>` or register a global exception middleware that logs via your adapter.
- You can log `Result` failures explicitly in handlers:
```csharp
if (result.IsFailure)
{
    _logger.Warn("Use case failed", new Dictionary<string, object?>
    {
        ["Errors"] = result.Errors.Select(e => new { e.Code, e.Message, e.TraceId }),
        ["CorrelationId"] = result.TraceId ?? _logContext.CorrelationId
    });
    return result;
}
```

## Properties and scopes
- `ILogContext.PushProperty` returns an `IDisposable` scope; use `using` statements to ensure pop happens even on exceptions.
- `InMemoryLogContext` keeps a dictionary of properties; it’s primarily for tests/diagnostics. For production, adapt to your logger’s native scope API (`BeginScope`, `LogContext.PushProperty`, etc.).
- `NoOpLogContext` is safe for scenarios where you don’t care about correlation or scopes (e.g., simple console apps).

## Configuration touchpoints
Logging abstractions themselves aren’t configured via `CoreExtensionsOptions`, but they participate in options used by pipeline behaviors:
- `CoreExtensionsOptions.CorrelationHeaderName` and `CorrelationIdFactory` affect correlation handling.
- `CoreExtensionsOptions.EnablePerformanceLogging` and `PerformanceWarningThreshold` influence `PerformanceBehavior` logs.
Set these in `appsettings` under `Extensions:Core`:
```json
{
  "Extensions": {
    "Core": {
      "CorrelationHeaderName": "X-Correlation-ID",
      "EnablePerformanceLogging": true,
      "PerformanceWarningThreshold": "00:00:00.500"
    }
  }
}
```

## Real-world patterns

### Enriching logs with user/tenant without coupling handlers
```csharp
public sealed class UserAwareLoggerAdapter<T> : IAppLogger<T>
{
    private readonly IAppLogger<T> _inner;
    private readonly ICurrentUser _currentUser;

    public UserAwareLoggerAdapter(IAppLogger<T> inner, ICurrentUser currentUser)
    {
        _inner = inner;
        _currentUser = currentUser;
    }

    public void Log(LogLevel level, string message, Exception? exception = null, IReadOnlyDictionary<string, object?>? properties = null)
    {
        var enriched = new Dictionary<string, object?>(properties ?? new Dictionary<string, object?>())
        {
            ["UserId"] = _currentUser.Id,
            ["UserName"] = _currentUser.UserName
        };
        _inner.Log(level, message, exception, enriched);
    }
}
```
- Register `UserAwareLoggerAdapter<T>` as a decorator over your provider adapter.

### Logging domain events with correlation
```csharp
public sealed class DomainEventLoggingDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly IAppLogger<DomainEventLoggingDispatcher> _logger;
    private readonly ILogContext _context;

    public DomainEventLoggingDispatcher(IMediator mediator, IAppLogger<DomainEventLoggingDispatcher> logger, ILogContext context)
    {
        _mediator = mediator;
        _logger = logger;
        _context = context;
    }

    public async Task DispatchAsync(IEnumerable<DomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var e in events)
        {
            _logger.Info("Publishing domain event", new Dictionary<string, object?>
            {
                ["EventType"] = e.GetType().Name,
                ["CorrelationId"] = e.CorrelationId ?? _context.CorrelationId
            });
            await _mediator.Publish(e, cancellationToken);
        }
    }
}
```
- Correlation flows from `ILogContext` to domain events, keeping observability aligned.

### Quiet mode for tests or local dev
- Use `NoOpAppLogger<T>` and `NoOpLogContext` to suppress logs entirely.
- Switch to `InMemoryAppLogger` when you want to assert on log emission without external sinks.

## Migration guidance from the template
- Keep using MEL/Serilog in Infrastructure, but inject adapters for `IAppLogger<T>` and `ILogContext`.
- Replace usages of `ILogger<T>` in new behaviors with `IAppLogger<T>` to avoid leaking provider dependencies into Application layer. Existing behaviors from the template can stay until you swap them for Core equivalents.
- In controllers or middleware, if you rely on `ILogger`, you can still do so; `IAppLogger` is primarily for Application layer and behaviors.

## Testing tips
- With `InMemoryAppLogger`, assert:
  - `Entries.Count` for expected log count.
  - `Entries.Any(e => e.Level == LogLevel.Warning && e.Message.Contains("Long running"))`.
  - `CorrelationId` presence matches `ILogContext.CorrelationId`.
- Use `FrozenClock` to make time-based properties deterministic when your adapter adds timestamps.
- When testing correlation middleware + behaviors end-to-end, set `ILogContext.CorrelationId` in middleware and ensure behavior logs carry the same value.

## FAQ
- **Do I have to abandon `ILogger`?** No. Use adapters so Application code stays on `IAppLogger`; Infrastructure can still be MEL/Serilog. Existing `ILogger` usages in Web/Infrastructure remain valid.
- **Where should I store correlation ID?** In `ILogContext.CorrelationId`. Set it from middleware or let `CorrelationBehavior` generate it. Adapters should read it when creating scopes.
- **Can I add scopes besides correlation?** Yes—`PushProperty` is generic. Add tenant ID, user ID, feature flag states, etc. Ensure sensitive data policies are followed.
- **What about OpenTelemetry?** Bridge `CorrelationId` to trace/span IDs in your adapter or middleware. The Core abstractions avoid a hard dependency on OTEL packages.
- **Do the abstractions log JSON automatically?** No. Serialization is handled by your provider. Properties are provided as structured objects; your adapter decides how to render them.

## Adoption checklist
1) Register `ILogContext` and `IAppLogger<T>` implementations (NoOp/InMemory or provider adapters).
2) Ensure `CorrelationBehavior` and `LoggingBehavior` are registered so correlation and lifecycle logs flow.
3) Add middleware (optional) to source correlation IDs from incoming requests.
4) Decorate your logger adapter if you need user/tenant enrichment.
5) For tests, swap `IAppLogger<T>` with `InMemoryAppLogger<T>` and assert on `LogEntry` data.
6) Gradually replace direct `ILogger` usage in new Application components with `IAppLogger` to keep the Application layer decoupled from provider specifics.

## Related docs
- [Core pipeline behaviors](./core-pipeline-behaviors.md) for correlation/logging/performance behaviors that consume these abstractions.
- [Core result primitives](./core-result-primitives.md) to see how trace IDs in results/errors align with correlation IDs in logs.
- [Core guard clauses](./core-guard-clauses.md) for guards that emit errors you might log with correlation.
- [Core extension overview](./core.md) for the big-picture view and registration guidance.
