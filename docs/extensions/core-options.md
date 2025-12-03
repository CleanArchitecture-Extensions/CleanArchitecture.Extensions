# Core Options

The Core extension exposes a single options class—`CoreExtensionsOptions`—to configure correlation, guard behavior, and performance logging. While Jason Taylor’s Clean Architecture template wires MediatR behaviors without centralized options, the Core package adds a cohesive configuration surface so you can control cross-cutting behavior from appsettings or DI. This page explains the template baseline, what options exist, how they affect guards, logging, pipeline behaviors, and how to use them in different environments (dev, test, prod).

## Template baseline
- **Behaviors:** Template MediatR behaviors (`LoggingBehaviour`, `AuthorizationBehaviour`, `ValidationBehaviour`, `PerformanceBehaviour`, `UnhandledExceptionBehaviour`) are hard-coded—no central options for thresholds or correlation.
- **Correlation:** No explicit correlation header or ID factory; logging relies on `ILogger<T>` scopes populated with user info.
- **Guards:** Template doesn’t ship guard options; validation uses FluentValidation with exception throwing.

## What Core configures
`CoreExtensionsOptions` lives in `CleanArchitecture.Extensions.Core.Options` and governs:
- `CorrelationHeaderName` (string, default `"X-Correlation-ID"`)
- `GuardStrategy` (enum, default `ReturnFailure`)
- `EnablePerformanceLogging` (bool, default `true`)
- `PerformanceWarningThreshold` (TimeSpan, default 500 ms)
- `CorrelationIdFactory` (`Func<string>`, default GUID “N”)
- `TraceId` (string?, default `null`)

## How options are used across Core
- **Correlation behaviors:** `CorrelationBehavior` uses `CorrelationIdFactory` when `ILogContext.CorrelationId` is missing. Controllers/middleware can use `CorrelationHeaderName` to read/write headers.
- **Logging behavior:** Uses `CorrelationBehavior`-set IDs; if none are set, it can seed correlation via `IClock.NewGuid()`—you can align that with `CorrelationIdFactory` for consistency.
- **Performance behavior:** Respects `EnablePerformanceLogging` and `PerformanceWarningThreshold` to toggle logging and control warnings vs. debug logs.
- **Guards:** `GuardOptions.FromOptions` maps `GuardStrategy` and `TraceId` into guard calls; you can supply an error sink for accumulation.
- **Results:** `TraceId` can be propagated to Result/Errors for consistent tracing between logs, domain events, and HTTP responses.

## Configuration via appsettings
```json
{
  "Extensions": {
    "Core": {
      "CorrelationHeaderName": "X-Correlation-ID",
      "GuardStrategy": "ReturnFailure",
      "EnablePerformanceLogging": true,
      "PerformanceWarningThreshold": "00:00:00.500",
      "TraceId": null
    }
  }
}
```
- Strings like `"ReturnFailure"` bind to `GuardStrategy`.
- TimeSpan format is standard .NET (`hh:mm:ss.fff`).
- `CorrelationIdFactory` cannot be bound via configuration; set it in code when registering options.

## Registering options in DI
```csharp
services.Configure<CoreExtensionsOptions>(configuration.GetSection("Extensions:Core"));
```
Override programmatically when needed:
```csharp
services.PostConfigure<CoreExtensionsOptions>(options =>
{
    options.CorrelationIdFactory = () => $"svc-{Guid.NewGuid():N}";
    options.PerformanceWarningThreshold = TimeSpan.FromMilliseconds(250);
});
```

## Environment-specific guidance
- **Development:** Keep `EnablePerformanceLogging = true` and threshold at 500 ms for visibility. Consider human-readable correlation IDs (`dev-{Guid}`) for quick searches.
- **Production:** Standardize `CorrelationHeaderName` with your API gateway. Keep thresholds aligned with SLOs; raise if noise is high. Ensure correlation IDs are opaque (GUIDs) to avoid leaking info.
- **Tests:** Inject `TraceId` for deterministic assertions. You can turn off performance logging to reduce noise or set a very low threshold to assert warnings in tests.

## Using options with guards
```csharp
var guardOptions = GuardOptions.FromOptions(coreOptions.Value);
var result = Guard.AgainstNullOrWhiteSpace(name, nameof(name), guardOptions);
if (result.IsFailure) return Result.Failure<string>(result.Errors, result.TraceId);
```
- `GuardStrategy` drives return/throw/accumulate.
- `TraceId` flows into errors produced by guards.
- For accumulation, pass an `ErrorSink` into `GuardOptions.FromOptions(options, sink)`.

## Using options with pipeline behaviors
```csharp
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CorrelationBehavior<,>));
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.Configure<CoreExtensionsOptions>(configuration.GetSection("Extensions:Core"));
```
- PerformanceBehavior will skip logging if `EnablePerformanceLogging` is false.
- CorrelationBehavior will use `CorrelationIdFactory` when no correlation ID is present.

## Patterns and scenarios

### 1) Matching gateway/header expectations
If your API gateway uses `X-Request-ID`, align it:
```csharp
services.Configure<CoreExtensionsOptions>(options =>
{
    options.CorrelationHeaderName = "X-Request-ID";
    options.CorrelationIdFactory = () => Guid.NewGuid().ToString("N");
});
```
Ensure middleware echoes the same header; `CorrelationBehavior` will reuse IDs set in `ILogContext`.

### 2) Tightening performance thresholds for hot paths
```csharp
services.Configure<CoreExtensionsOptions>(options =>
{
    options.PerformanceWarningThreshold = TimeSpan.FromMilliseconds(200);
});
```
Use feature-specific options (named options) if some handlers are expected to run longer (e.g., reporting).

### 3) Disabling performance logs for chatty background jobs
```csharp
services.Configure<CoreExtensionsOptions>(options =>
{
    options.EnablePerformanceLogging = false;
});
```
This keeps noise down in queues/cron workers that run high-frequency MediatR requests.

### 4) Correlation ID strategy per environment
- Dev: prefix with `dev-` for quick grep (`dev-{Guid}`).
- Staging/prod: opaque GUIDs.
Set `CorrelationIdFactory` accordingly in environment-specific DI modules.

### 5) Seeding TraceId for result/guard flows
For APIs that already have a trace/correlation token (e.g., from a reverse proxy):
```csharp
services.Configure<CoreExtensionsOptions>(options =>
{
    options.TraceId = "injected-from-middleware"; // or set per-request in middleware using IOptionsSnapshot
});
```
Better: set `TraceId` per request using `IOptionsSnapshot<CoreExtensionsOptions>` and a middleware that copies the incoming header value into options for that scope.

## Per-request overrides
Use `IOptionsSnapshot<CoreExtensionsOptions>` or a scoped options wrapper to set per-request values (e.g., TraceId) based on incoming HTTP headers:
```csharp
public class CorrelationOptionsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOptionsSnapshot<CoreExtensionsOptions> _options;
    private readonly ILogContext _context;

    public CorrelationOptionsMiddleware(RequestDelegate next, IOptionsSnapshot<CoreExtensionsOptions> options, ILogContext context)
    {
        _next = next;
        _options = options;
        _context = context;
    }

    public async Task Invoke(HttpContext http)
    {
        var incoming = http.Request.Headers[_options.Value.CorrelationHeaderName].FirstOrDefault();
        var correlationId = string.IsNullOrWhiteSpace(incoming) ? _options.Value.CorrelationIdFactory() : incoming;
        _context.CorrelationId = correlationId;
        await _next(http);
    }
}
```
- For `TraceId`, consider a scoped service that copies `_context.CorrelationId` into `CoreExtensionsOptions.TraceId` if you need Results to carry it automatically.

## Testing options
- Use `Options.Create(new CoreExtensionsOptions { ... })` for unit tests of behaviors/guards.
- For performance logs, set `EnablePerformanceLogging = true` and a low threshold, then assert that `InMemoryAppLogger` captures warnings.
- For guard tests, set `TraceId` and assert it flows into `Error.TraceId`.

## Migration tips from the template
- Keep template behaviors if desired, but wire `CoreExtensionsOptions` to control Core behaviors. Template behaviors ignore these options, so ensure you register Core behaviors to benefit from the settings.
- If you currently hard-code thresholds or headers, move them into `Extensions:Core` configuration and consume via DI.
- Replace ad hoc correlation ID generation with `CorrelationIdFactory` to centralize ID format.

## Reference: option defaults and effects
- `CorrelationHeaderName`: Used by middleware/controllers to read/write correlation; behavior uses `ILogContext` (not header directly).
- `CorrelationIdFactory`: Used by `CorrelationBehavior` when `ILogContext.CorrelationId` is empty.
- `GuardStrategy`: Default for `GuardOptions.FromOptions`; controls return vs throw vs accumulate.
- `EnablePerformanceLogging`: If false, `PerformanceBehavior` becomes a pass-through.
- `PerformanceWarningThreshold`: Controls Warn vs Debug logging in `PerformanceBehavior`.
- `TraceId`: Applied to guards/results when provided; useful for aligning API trace tokens with internal errors.

## Adoption checklist
1) Bind `Extensions:Core` in configuration and call `services.Configure<CoreExtensionsOptions>(...)`.
2) Decide on correlation header and ID format; update gateway/middleware accordingly.
3) Set `GuardStrategy` globally; override per-call with `GuardOptions` when needed.
4) Tune `PerformanceWarningThreshold` and `EnablePerformanceLogging` per environment.
5) If you need TraceId propagation, set it per request (middleware + options snapshot) and pass it into Results/Errors.

## Related docs
- [Core pipeline behaviors](./core-pipeline-behaviors.md) for correlation/performance logging powered by these options.
- [Core guard clauses](./core-guard-clauses.md) for guard behavior controlled via options.
- [Core result primitives](./core-result-primitives.md) to see how TraceId from options flows into results/errors.
- [Core logging abstractions](./core-logging-abstractions.md) for correlation scopes influenced by header/factory choices.
- [Core extension overview](./core.md) for the big-picture registration instructions.
