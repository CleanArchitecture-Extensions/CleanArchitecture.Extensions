# Installation

Install the shipping extensions (Core and Validation), wire them into a Jason Taylor Clean Architecture solution, and set up optional local tooling for docs. This page is practical and mirrors the README; copy/paste is encouraged.

## Prerequisites

- .NET SDK `8.x` or `10.x` (building/running extensions, samples, and tests).
- Git (if you are cloning this repo).
- A solution created from [Jason Taylor's template](https://github.com/jasontaylordev/CleanArchitecture) (`dotnet new clean-architecture` or install the template first).
- Optional for docs work: Python 3.10+ if you plan to build/serve the MkDocs site locally.

## Install Core

Add the package to your Application project (adjust project path/name as needed):

```powershell
dotnet add src/YourProject.Application/YourProject.Application.csproj package CleanArchitecture.Extensions.Core --version 0.1.1-preview.1
```

Register pipeline behaviors (order mirrors the template: Correlation → Logging pre-processor → UnhandledException → Authorization → Validation → Performance → Handler):

```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CorrelationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
```

Optional configuration (see [extensions/core-options.md](../extensions/core-options.md)):

```csharp
services.Configure<CoreExtensionsOptions>(options =>
{
    options.CorrelationIdHeader = "X-Correlation-ID";
    options.PerformanceThresholdMilliseconds = 250;
    options.DefaultGuardStrategy = GuardStrategy.ReturnResult;
});
```

## Install Validation (if you use FluentValidation)

Add the package:

```powershell
dotnet add src/YourProject.Application/YourProject.Application.csproj package CleanArchitecture.Extensions.Validation --version 0.1.1-preview.1
```

Register validators and the behavior:

```csharp
services.AddValidatorsFromAssemblyContaining<CreateOrderValidator>();
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
```

Configure behavior strategy (see [extensions/validation.md](../extensions/validation.md)):

```csharp
services.Configure<ValidationOptions>(options =>
{
    options.Strategy = ValidationStrategy.ReturnResult; // Throw | ReturnResult | Notify
    options.IncludePropertyName = true;
    options.LogValidationFailures = true;
    options.SeverityLogLevels[Severity.Error] = LogLevel.Warning;
});
```

## Restore, build, and run

- Restore packages: `dotnet restore`
- Build: `dotnet build`
- Run your API (or the project that triggers MediatR):\
  `dotnet run --project src/YourProject.Api/YourProject.Api.csproj`
- Trigger a request and check logs for correlation IDs, performance warnings, and validation results.

If you want a known-good baseline, run a sample from this repo:

```powershell
dotnet run --project samples/CleanArchitecture.Extensions.Core.Pipeline.Sample/CleanArchitecture.Extensions.Core.Pipeline.Sample.csproj
```

## Uninstall or rollback

Because everything is opt-in, you can remove an extension cleanly:

```powershell
dotnet remove src/YourProject.Application/YourProject.Application.csproj package CleanArchitecture.Extensions.Core
dotnet remove src/YourProject.Application/YourProject.Application.csproj package CleanArchitecture.Extensions.Validation
```

Then remove the corresponding DI registrations and option bindings. Your solution returns to the baseline template behavior.

## Local docs tooling (optional)

If you want to build or serve the docs site locally:

```powershell
python -m venv .venv
. .venv/Scripts/Activate.ps1
pip install -r docs/requirements.txt
mkdocs serve
```

Visit the local URL printed by MkDocs (default http://127.0.0.1:8000/) to preview changes.

## Troubleshooting

- **Package not found**: ensure you are on .NET 8 or 10 and that your NuGet sources can reach the `CleanArchitecture.Extensions.*` preview packages.
- **Correlation ID missing**: confirm `CorrelationBehavior` is registered before logging/performance behaviors and that your logging provider includes scopes.
- **Validation not firing**: ensure validators are in the assembly you scan with `AddValidatorsFromAssemblyContaining<...>` and that `ValidationBehaviour` is registered.
- **Too many performance warnings**: adjust `PerformanceThresholdMilliseconds` or filter specific requests in logging configuration.
- **Legacy Result migration**: use `LegacyResult`/`LegacyResult<T>` from Core to bridge the template's result shape while you migrate handlers.

## Where to go next

- Quickstart (step-by-step wiring with expected output): [quickstart.md](quickstart.md)
- Core deep dives: [extensions/core.md](../extensions/core.md) · [core-result-primitives](../extensions/core-result-primitives.md) · [core-pipeline-behaviors](../extensions/core-pipeline-behaviors.md)
- Validation deep dive: [extensions/validation.md](../extensions/validation.md)
- Concepts: [architecture fit](../concepts/architecture-fit.md) · [composition & invariants](../concepts/composition.md)
- Recipes and samples: [recipes/authentication.md](../recipes/authentication.md) · [recipes/caching.md](../recipes/caching.md) · [samples/index.md](../samples/index.md)
