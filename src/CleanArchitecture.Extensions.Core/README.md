# CleanArchitecture.Extensions.Core

Core primitives for Clean Architecture apps built on MediatR.

- Pipeline behaviors for logging, performance timing, and correlation IDs.
- Result model with trace identifiers, error aggregation, and success/failure helpers.
- Domain event support, guard helpers, and clock abstractions for deterministic tests.
- Legacy template shims (`LegacyResult`/`LegacyResult<T>`) to ease migration from Jason Taylor’s `Result` shape.
- Ships with SourceLink, XML docs, and snupkg symbols for a smooth debugging experience.

## Install

```powershell
dotnet add package CleanArchitecture.Extensions.Core --version 0.1.0-preview.1
```

## Usage

Register the pipeline behaviors you need:

```csharp
using CleanArchitecture.Extensions.Core.Behaviors;
using MediatR;

services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CorrelationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
```

Work with results:

```csharp
using CleanArchitecture.Extensions.Core.Results;

var result = Result.Success(order, Activity.Current?.Id);

if (result.IsFailure)
{
    return Results.BadRequest(result.Errors);
}

// Map to/from template-style Result shape during migration.
var legacy = LegacyResult.FromResult(result);
var backToRich = legacy.ToResult(result.TraceId);
```

## Target frameworks

- net8.0
- net10.0
