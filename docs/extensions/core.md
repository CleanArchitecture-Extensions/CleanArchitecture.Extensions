# Extension: Core

## Overview
Foundation helpers inspired by Jason Taylor's Clean Architecture template: guard clauses, rich `Result`/`Error`, domain event primitives, logging abstractions with correlation, clocks, and MediatR pipeline behaviors for correlation, logging, and performance timing.

## When to use
- You want consistent guard clauses and `Result` semantics across extensions and features.
- You need correlation-aware logging and MediatR behaviors without pulling in a full logging stack.
- You prefer testable abstractions for time, logging, and domain events.

## Prereqs & Compatibility
- Target frameworks: `net10.0` (current).
- Dependencies: MediatR `12.2.0`.
- Template fit: Jason Taylor Clean Architecture (add MediatR pipeline behaviors in the Application layer).

## Install
```bash
dotnet add src/YourProject/YourProject.csproj package CleanArchitecture.Extensions.Core
```

## Usage

### Wire up basics (DI)
Register the abstractions and pipeline behaviors in your Application layer:
```csharp
services.AddSingleton<IClock, SystemClock>();
services.AddScoped<ILogContext, InMemoryLogContext>(); // replace with adapter to your logger later
services.AddScoped(typeof(IAppLogger<>), typeof(NoOpAppLogger<>)); // or your own implementation

services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CorrelationBehavior<,>));
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

services.Configure<CoreExtensionsOptions>(configuration.GetSection("Extensions:Core"));
```

### Guard clauses + Result
```csharp
var result = Guard.AgainstNullOrWhiteSpace(name, nameof(name), new GuardOptions
{
    Strategy = GuardStrategy.ReturnFailure,
    TraceId = "req-123"
});

if (result.IsFailure)
{
    return Result.Failure(result.Errors, result.TraceId);
}
```
- Strategies: `ReturnFailure` (default), `Throw`, `Accumulate` (collect errors into a sink).
- Guards emit `Error` objects with codes/messages and propagate trace IDs.

### MediatR behaviors
- `CorrelationBehavior`: ensures `CorrelationId` in `ILogContext` (uses `CoreExtensionsOptions.CorrelationIdFactory` fallback).
- `LoggingBehavior`: logs start/finish with correlation scope.
- `PerformanceBehavior`: times handlers, warns when `PerformanceWarningThreshold` is exceeded; toggle with `EnablePerformanceLogging`.

### Logging abstractions
- `IAppLogger<T>` and `ILogContext` let you plug in your logging provider. Provided implementations:
  - `NoOpAppLogger<T>` for silent scenarios/tests.
  - `InMemoryAppLogger<T>` and `InMemoryLogContext` for tests/diagnostics.

### Time
- `IClock` abstraction with `SystemClock`, `OffsetClock`, and `FrozenClock` (for deterministic tests).

### Domain events
- `DomainEvent` base type (`INotification`), `DomainEventTracker` to collect/flush events, and `IDomainEventDispatcher` abstraction for your dispatcher implementation.

## Configuration
`Extensions:Core` section (example):
```json
{
  "Extensions": {
    "Core": {
      "CorrelationHeaderName": "X-Correlation-ID",
      "GuardStrategy": "ReturnFailure",
      "EnablePerformanceLogging": true,
      "PerformanceWarningThreshold": "00:00:00.500"
    }
  }
}
```

## Troubleshooting
- No correlation ID in logs: ensure `CorrelationBehavior` and `ILogContext` are registered.
- Performance logs missing: set `EnablePerformanceLogging` to `true` and ensure `PerformanceBehavior` is in the pipeline.
- Guard exceptions thrown: set `GuardStrategy` to `ReturnFailure` or `Accumulate` if you don’t want exceptions.

## Samples & Tests
- Hook into application-level samples once available; for now, use the snippets above as the minimal integration path.
