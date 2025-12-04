# Core Pipeline Behaviors

The Core extension ships MediatR behaviors that drop into Jason Taylor’s Clean Architecture pipeline without breaking existing registrations. This page explains what the template already includes, why the Core behaviors add correlation- and telemetry-friendly enhancements, and how to wire, order, and use them in real projects. Examples show both the pre-processor signature (`IRequestPreProcessor<TRequest>`) and the standard `IPipelineBehavior<TRequest, TResponse>` pattern so you can stay compatible with the template’s DI setup.

## What the template already covers
In the template (`src/Application/Common/Behaviours`), MediatR is wired with:
- `LoggingBehaviour<TRequest>` (`IRequestPreProcessor<TRequest>`): Logs request name, user ID, and user name using `ILogger<TRequest>`, `IUser`, and `IIdentityService>`.
- `UnhandledExceptionBehaviour<TRequest, TResponse>`: Catches exceptions, logs them, and rethrows.
- `AuthorizationBehaviour<TRequest, TResponse>`: Enforces `AuthorizeAttribute` roles/policies using `IUser` and `IIdentityService`; throws `UnauthorizedAccessException`/`ForbiddenAccessException`.
- `ValidationBehaviour<TRequest, TResponse>`: Runs FluentValidation validators; throws `ValidationException` when failures exist.
- `PerformanceBehaviour<TRequest, TResponse>`: Uses a `Stopwatch`; logs warnings via `ILogger<TRequest>` when elapsed time > 500 ms. It does not attach correlation metadata.

Registration order in `Application.DependencyInjection`: pre-processor logging, then unhandled exception, authorization, validation, performance.

Gaps the template intentionally leaves open:
- No correlation ID propagation across logs and handlers.
- No structured logging abstraction—everything is tied to `ILogger<T>`.
- No toggle for performance logging or threshold configuration via options.
- No reusable logging scope that flows into other behaviors.

## What the Core behaviors add
The Core behaviors preserve template compatibility while adding cross-cutting concerns you need in production:
- **Correlation-aware:** `CorrelationBehavior` ensures `ILogContext.CorrelationId` is set (configurable factory) and pushes it into a logging scope.
- **Structured logging:** `LoggingBehavior` emits start/finish events and also implements `IRequestPreProcessor<TRequest>` so you can register it exactly like the template. It uses `IAppLogger<T>` and `ILogContext` for provider-agnostic structured logs and correlation.
- **Configurable performance telemetry:** `PerformanceBehavior` measures elapsed time with `IClock` and warns when `CoreExtensionsOptions.PerformanceWarningThreshold` is exceeded; it can be globally disabled via `EnablePerformanceLogging`.
- **Option-driven defaults:** Correlation header/name, ID factory, and performance thresholds live in `CoreExtensionsOptions`.
- **Provider-agnostic logging:** Behaviors depend on `IAppLogger<T>` and `ILogContext`, making it easy to plug in Serilog, MEL, or in-memory loggers for tests.

## Behavior APIs (Core)
- `CorrelationBehavior<TRequest, TResponse>` (`IPipelineBehavior`): Ensures correlation ID, pushes scope via `ILogContext.PushProperty`.
- `LoggingBehavior<TRequest, TResponse>` (`IPipelineBehavior` + `IRequestPreProcessor<TRequest>`): Logs start/end with correlation + request type; sets correlation if missing.
- `PerformanceBehavior<TRequest, TResponse>` (`IPipelineBehavior`): Times handler execution; warns vs. debug logs; respects `EnablePerformanceLogging` and `PerformanceWarningThreshold`.

Dependencies:
- `IAppLogger<T>` (logging abstraction).
- `ILogContext` (correlation scope).
- `IClock` (time source).
- `CoreExtensionsOptions` (correlation + performance settings).

## Recommended pipeline order
To stay compatible with the template’s semantics while adding correlation:
1) `CorrelationBehavior` (ensures correlation ID).
2) `LoggingBehavior` registered as `IRequestPreProcessor` (logs start) and as `IPipelineBehavior` (logs end).
3) `UnhandledExceptionBehaviour` (template).
4) `AuthorizationBehaviour` (template).
5) `ValidationBehaviour` (template).
6) `PerformanceBehaviour` (measures the whole request).
7) Handler.

This preserves the template’s order while guaranteeing that correlation and logging scopes exist for subsequent behaviors and for performance logs.

## Wiring in DI (Application layer)
A runnable example lives at `samples/CleanArchitecture.Extensions.Core.Pipeline.Sample/src/Application/DependencyInjection.cs`. It wires Core behaviors plus adapters that bridge to `ILogger<T>`:

```csharp
builder.Services.Configure<CoreExtensionsOptions>(builder.Configuration.GetSection("Extensions:Core"));
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<ILogContext, MelLogContext>(); // MEL-backed scope
builder.Services.AddScoped(typeof(IAppLogger<>), typeof(MelAppLogger<>)); // MEL adapter

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.AddOpenRequestPreProcessor(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(CorrelationBehavior<,>));
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
    cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
    cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
});
```
- The adapter types (`MelLogContext`, `MelAppLogger<T>`) show how to reuse existing `ILogger` pipelines while satisfying Core abstractions.
- `LoggingBehavior` is registered twice: once as a pre-processor (start log) and once as a pipeline behavior (end log).

## Sample-backed walkthrough (pipeline sample)
The runnable sample at `samples/CleanArchitecture.Extensions.Core.Pipeline.Sample` exercises the behaviors with real endpoints.

### Correlation flowing into handlers
`src/Application/Diagnostics/Queries/GetPipelineDiagnostics/GetPipelineDiagnostics.cs`:
```csharp
var correlationId = _logContext.CorrelationId ?? _clock.NewGuid().ToString("N");

_logger.Log(LogLevel.Information, $"Diagnostics requested with correlation {correlationId}");

return Task.FromResult(new PipelineDiagnosticsDto(correlationId, _clock.UtcNow));
```
- `CorrelationBehavior` seeds `_logContext.CorrelationId`; the handler reuses it and returns it to the caller.

### Performance warnings on slow commands
`src/Application/Diagnostics/Commands/SimulateWork/SimulateWork.cs`:
```csharp
var delay = Math.Max(0, request.Milliseconds);
_logger.Log(LogLevel.Information, $"Simulating {delay} ms of work");

await _clock.Delay(TimeSpan.FromMilliseconds(delay), cancellationToken);

_logger.Log(LogLevel.Debug, $"Completed simulated work after {delay} ms");
```
- With `PerformanceWarningThreshold` set to 400 ms in `src/Web/appsettings.json`, `POST /api/Diagnostics/simulate?milliseconds=650` emits a performance warning from `PerformanceBehavior` while returning 202 Accepted.

### Minimal API endpoints exposing the behaviors
`src/Web/Endpoints/Diagnostics.cs`:
```csharp
public async Task<IResult> GetPipelineDiagnostics(ISender sender) =>
    TypedResults.Ok(await sender.Send(new GetPipelineDiagnosticsQuery()));

public async Task<IResult> SimulateWork(ISender sender, int milliseconds = 600)
{
    await sender.Send(new SimulateWorkCommand(milliseconds));
    return TypedResults.Accepted($"/api/{nameof(Diagnostics)}/simulate?milliseconds={milliseconds}");
}
```
- `LoggingBehavior` logs start/end, `CorrelationBehavior` scopes correlation, and `PerformanceBehavior` times the simulated work.

## Behavior-by-behavior deep dive

### CorrelationBehavior
- **Purpose:** Ensure every request has a correlation ID flowing through `ILogContext` and attached to structured logs.
- **How it works:** If `ILogContext.CorrelationId` is empty, it uses `CoreExtensionsOptions.CorrelationIdFactory` (default GUID) or `_clock.NewGuid()` to create one. It pushes a property named `CorrelationId` into the logging scope via `ILogContext.PushProperty`, then calls `next`.
- **Interop with web APIs:** Controllers or middleware can set `ILogContext.CorrelationId` from incoming headers (e.g., `X-Correlation-ID`). The behavior preserves that value rather than overwriting it.
- **Why before logging:** Downstream behaviors and handlers can rely on `ILogContext.CorrelationId` being present for logs, telemetry, and Results.

### LoggingBehavior
- **Purpose:** Emit structured logs at start and end of request handling, carrying correlation ID and request type.
- **Dual interface:** Implements `IPipelineBehavior` and `IRequestPreProcessor<TRequest>` so it can plug into both the template’s pre-processor registration (`cfg.AddOpenRequestPreProcessor`) and standard pipeline registration.
- **Scope handling:** Uses `ILogContext.PushProperty` to keep `CorrelationId` in scope for downstream logging and for the paired end log.
- **Data logged:** Request type (full name), correlation ID, start timestamp (from `IClock`), and lifecycle messages (“Starting”, “Handling”, “Handled”).
- **Compatibility:** Replaces the template `LoggingBehaviour<TRequest>`; you can still enrich logs with user info by adapting `IAppLogger<T>` to include user claims.

### PerformanceBehavior
- **Purpose:** Measure elapsed time for each request and emit warnings above a configurable threshold; otherwise debug-level messages.
- **Config:** `CoreExtensionsOptions.EnablePerformanceLogging` (bool) and `PerformanceWarningThreshold` (TimeSpan, default 500 ms).
- **Correlation:** Includes `CorrelationId` from `ILogContext` in logged properties.
- **Clock:** Uses `IClock` to allow deterministic testing with `FrozenClock`.
- **Behavior:** If logging is disabled, it immediately forwards to `next`. Otherwise, it records start time, invokes `next`, computes elapsed, and logs either Warn (over threshold) or Debug (under threshold).

## Real-world usage patterns

### 1) Correlating API requests end-to-end
```csharp
public class CorrelationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogContext _logContext;
    private readonly CoreExtensionsOptions _options;

    public CorrelationMiddleware(RequestDelegate next, ILogContext logContext, IOptions<CoreExtensionsOptions> options)
    {
        _next = next;
        _logContext = logContext;
        _options = options.Value;
    }

    public async Task Invoke(HttpContext context)
    {
        var header = _options.CorrelationHeaderName;
        var incoming = context.Request.Headers[header].FirstOrDefault();
        _logContext.CorrelationId = string.IsNullOrWhiteSpace(incoming)
            ? _options.CorrelationIdFactory()
            : incoming;

        using var scope = _logContext.PushProperty("CorrelationId", _logContext.CorrelationId);
        context.Response.Headers[header] = _logContext.CorrelationId!;
        await _next(context);
    }
}
```
- With the middleware setting `ILogContext.CorrelationId`, `CorrelationBehavior` will reuse it, ensuring MediatR logs share the same ID.

### 2) Swapping logging providers without touching handlers
```csharp
// Adapter bridging IAppLogger<T> to Microsoft.Extensions.Logging
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
- Behaviors depend on `IAppLogger<T>`; swapping providers requires only registering a different adapter.

### 3) Measuring command performance with thresholds per feature
If some handlers are expected to run longer, you can override options per scope:
```csharp
public class FeaturePerformanceOptions
{
    public static CoreExtensionsOptions ForReporting(CoreExtensionsOptions baseOptions) =>
        new()
        {
            CorrelationHeaderName = baseOptions.CorrelationHeaderName,
            GuardStrategy = baseOptions.GuardStrategy,
            EnablePerformanceLogging = true,
            PerformanceWarningThreshold = TimeSpan.FromSeconds(2),
            CorrelationIdFactory = baseOptions.CorrelationIdFactory
        };
}
```
Inject a feature-specific options instance where needed (e.g., using named options) if you must raise the threshold for a known long-running report generation command.

### 4) Capturing correlation in Result/Error metadata
```csharp
public async Task<Result<Guid>> Handle(CreateInvoiceCommand request, CancellationToken ct)
{
    var traceId = _logContext.CorrelationId ?? Guid.NewGuid().ToString("N");

    var customer = await _customers.GetAsync(request.CustomerId, ct);
    if (customer is null)
    {
        return Result.Failure<Guid>(new Error("invoice.customer-not-found", "Customer not found", traceId));
    }

    var invoiceId = await _invoices.CreateAsync(request.CustomerId, request.Amount, ct);
    return Result.Success(invoiceId, traceId);
}
```
- Because `CorrelationBehavior` set `ILogContext.CorrelationId`, the handler can propagate the same ID into `Result`/`Error` for downstream API responses.

### 5) Testing behaviors deterministically
```csharp
[Fact]
public async Task PerformanceBehavior_Warns_When_Over_Threshold()
{
    var clock = new FrozenClock(DateTimeOffset.Parse("2025-01-01T00:00:00Z"));
    var logger = new InMemoryAppLogger<TestRequest>();
    var context = new InMemoryLogContext { CorrelationId = "corr-1" };
    var options = Microsoft.Extensions.Options.Options.Create(new CoreExtensionsOptions { EnablePerformanceLogging = true, PerformanceWarningThreshold = TimeSpan.FromMilliseconds(10) });
    var behavior = new PerformanceBehavior<TestRequest, Unit>(logger, context, options, clock);

    var response = await behavior.Handle(new TestRequest(), _ => Task.FromResult(Unit.Value), CancellationToken.None);

    logger.Entries.Should().Contain(e => e.Level == LogLevel.Warning && e.CorrelationId == "corr-1");
}
```
- `FrozenClock` and `InMemoryAppLogger` make assertions straightforward.

## Configuration reference
`CoreExtensionsOptions` fields relevant to behaviors:
- `CorrelationHeaderName`: The header your APIs use for correlation (helps middleware + controllers align).
- `CorrelationIdFactory`: Func<string> to generate correlation IDs when absent (default GUID “N”).
- `EnablePerformanceLogging`: Toggle performance measurement.
- `PerformanceWarningThreshold`: TimeSpan for warning threshold (default 500 ms).
- `TraceId`: Optional trace ID to apply to guard/result flows (not directly used by behaviors but useful for consistency).

Example appsettings:
```json
{
  "Extensions": {
    "Core": {
      "CorrelationHeaderName": "X-Correlation-ID",
      "EnablePerformanceLogging": true,
      "PerformanceWarningThreshold": "00:00:00.500",
      "GuardStrategy": "ReturnFailure"
    }
  }
}
```

## Ordering and coexistence tips
- **Pre-processor vs. pipeline:** Keep `LoggingBehavior` registered as a pre-processor if you need “request started” logs before other behaviors. Its pipeline registration still logs “Handled” after downstream behaviors.
- **Validation exceptions:** If FluentValidation throws, `LoggingBehavior` will have already logged “Handling”; `UnhandledExceptionBehaviour` will log the exception. Consider adding error logs in exception middleware if you need both start/finish markers on failure.
- **Multiple correlation sources:** If API middleware sets `ILogContext.CorrelationId`, `CorrelationBehavior` will respect it. Avoid regenerating IDs in handlers; rely on the behavior + middleware.
- **Performance scope:** If you run background jobs with MediatR, performance logs will still flow; set `EnablePerformanceLogging = false` for noise-sensitive jobs or adjust thresholds.

## Migration from template behaviors
- Replace `LoggingBehaviour<TRequest>` with Core `LoggingBehavior` registrations.
- Keep `UnhandledExceptionBehaviour`, `AuthorizationBehaviour`, and `ValidationBehaviour` as-is; Core behaviors are additive.
- Remove `Stopwatch`-based performance behavior from the template if you want correlation-aware performance logging; Core `PerformanceBehavior` is a drop-in replacement with options.
- Keep handler code unchanged; behaviors are registered in DI and require no handler modifications.

## FAQ
- **Do I need both pre-processor and pipeline registrations for LoggingBehavior?** Optional but recommended for parity with the template: pre-processor captures the “starting” log before other behaviors run; pipeline behavior captures “handled” after all behaviors and the handler.
- **Can I add request payload sampling?** Wrap `IAppLogger<T>` to include sanitized payload summaries; keep PII concerns in mind. The behavior provides request type and correlation ID; payload logging is left to your adapter to avoid allocations and sensitivity issues.
- **What about OpenTelemetry?** Adapt `IAppLogger<T>` or `ILogContext` to bridge correlation IDs to trace/span IDs. The behaviors themselves do not depend on OTEL packages.
- **Does correlation flow to domain events?** If you propagate `ILogContext.CorrelationId` into `DomainEvent.CorrelationId` or `Result.TraceId` inside handlers, you can correlate across pipelines. Core behaviors set the context; you attach it to your domain events manually.

## Adoption checklist
1) Register `CorrelationBehavior`, `LoggingBehavior` (pre-processor + pipeline), and `PerformanceBehavior` in the Application DI layer.
2) Configure `CoreExtensionsOptions` (header name, correlation ID factory, performance threshold).
3) Provide `ILogContext` and `IAppLogger<T>` adapters that forward correlation scopes to your logging provider.
4) Add middleware (optional) to set `ILogContext.CorrelationId` from incoming headers and echo it back.
5) Validate ordering: ensure correlation precedes logging; ensure performance wraps handler work after validation/authorization.
6) Write a smoke test: send a request through MediatR in-memory, assert correlation ID is present in logs and performance logs respect thresholds.

## Related docs
- [Core extension overview](./core.md) for the full package and registration guidance.
- [Core Result Primitives](./core-result-primitives.md) to see how correlation flows into results/errors.
- [Core Guard Clauses](./core-guard-clauses.md) for input validation that pairs well with these behaviors.
- [Validation extension](./validation.md) if you lean on FluentValidation behaviors alongside Core pipeline behaviors.
