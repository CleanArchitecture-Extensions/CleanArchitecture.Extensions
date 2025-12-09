# Quickstart

Install the first two extensions (Core and Validation), wire them into a Jason Taylor Clean Architecture solution, and verify behavior in minutes. This page is copy/paste friendly and mirrors the README while keeping you in the docs site.

## What you will accomplish

- Start from Jason Taylor's Clean Architecture template (no forking).
- Add `CleanArchitecture.Extensions.Core` for correlation/logging/performance, Result primitives, guards, domain events, time abstractions, and options.
- Add `CleanArchitecture.Extensions.Validation` for FluentValidation in the MediatR pipeline with configurable strategies.
- Wire behaviors in the right order, configure options, and run a sample to see everything working.
- Know where to go next (docs, samples, recipes, roadmap).

## Prerequisites

- .NET SDK `8.x` or `10.x` installed.
- A solution created from [Jason Taylor's template](https://github.com/jasontaylordev/CleanArchitecture) (`dotnet new clean-architecture -n ...` or install the template first).
- An Application project that already uses MediatR and (optionally) FluentValidation.
- PowerShell or Bash available for running commands.

## Step 0 — open or create the solution

- If new: `dotnet new clean-architecture -n YourCompany.YourProduct`
- If existing: open your current template-based solution and continue.

Keep the upstream template untouched. Everything below is additive and reversible via package removal and DI registration changes.

## Step 1 — add Core

Install the package in your Application project (adjust path/name to your project):

```powershell
dotnet add src/YourProject.Application/YourProject.Application.csproj package CleanArchitecture.Extensions.Core --version 0.1.1-preview.1
```

Register pipeline behaviors (order matches the template: Correlation → Logging pre-processor → UnhandledException → Authorization → Validation → Performance → Handler):

```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CorrelationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
```

Optional: configure Core defaults (see [extensions/core-options.md](../extensions/core-options.md)):

```csharp
services.Configure<CoreExtensionsOptions>(options =>
{
    options.CorrelationIdHeader = "X-Correlation-ID";
    options.PerformanceThresholdMilliseconds = 250;
    options.DefaultGuardStrategy = GuardStrategy.ReturnResult;
});
```

What this enables immediately:

- Correlation IDs flow through pipeline logs.
- Logging behavior emits structured start/finish entries.
- Performance warnings appear when handlers exceed the threshold.
- Result/Result<T>, guard clauses, domain events, and time abstractions become available in Application code.

## Step 2 — add Validation (if you use FluentValidation)

Install the package:

```powershell
dotnet add src/YourProject.Application/YourProject.Application.csproj package CleanArchitecture.Extensions.Validation --version 0.1.1-preview.1
```

Register validators and the pipeline behavior:

```csharp
services.AddValidatorsFromAssemblyContaining<CreateOrderValidator>();
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
```

Configure strategy and logging (see [extensions/validation.md](../extensions/validation.md)):

```csharp
services.Configure<ValidationOptions>(options =>
{
    options.Strategy = ValidationStrategy.ReturnResult; // Throw | ReturnResult | Notify
    options.IncludePropertyName = true;
    options.LogValidationFailures = true;
    options.SeverityLogLevels[Severity.Error] = LogLevel.Warning;
});
```

What this enables immediately:

- Validators run early in the pipeline, with correlation-aware logging.
- You choose how failures surface: exception, Result, or notification publisher.
- Reusable rule catalog (`CommonRules`) for trimmed strings, email/phone, pagination, culture codes, and sort expressions.

## Step 3 — run and verify

Run your Application project or a sample handler to exercise the pipeline:

```powershell
dotnet run --project src/YourProject.Api/YourProject.Api.csproj
```

Trigger a request that hits MediatR. Check for:

- Correlation IDs present in logs.
- PerformanceBehavior warnings when you simulate slow handlers.
- Validation failures logged with property names when strategy is `ReturnResult` or `Notify`.

If you prefer to see a known-good sample first, run one of the bundled samples:

```powershell
dotnet run --project samples/CleanArchitecture.Extensions.Core.Pipeline.Sample/CleanArchitecture.Extensions.Core.Pipeline.Sample.csproj
```

Expected output (trimmed for clarity):

```
info: CorrelationBehavior[0] Request Starting CorrelationId=...
info: LoggingBehavior[0] Handling Ping
warn: PerformanceBehavior[0] Long Running Request: Ping (310ms) CorrelationId=...
info: LoggingBehavior[0] Handled Ping
```

## Step 4 — adjust for your environment

- **Ordering**: keep behaviors in the recommended order; add Authorization/Exception behaviors where your template already expects them.
- **Strategy**: choose `ValidationStrategy.Throw` for controller-friendly exceptions or `ReturnResult` for explicit handler outcomes.
- **Guard style**: set `GuardStrategy.Throw` or `GuardStrategy.ReturnResult` based on how you want to surface invariants.
- **Correlation header**: align `CoreExtensionsOptions.CorrelationIdHeader` with your gateway/load balancer conventions.
- **Performance threshold**: tune `PerformanceThresholdMilliseconds` per environment.

## Troubleshooting (fast fixes)

- **No correlation ID in logs**: ensure `CorrelationBehavior` is registered before logging/performance behaviors and your logger includes scopes.
- **Validation not firing**: confirm validators are in the same assembly you scan with `AddValidatorsFromAssemblyContaining<...>` and that `ValidationBehaviour` is registered.
- **Unexpected exceptions**: if you prefer results, switch `ValidationOptions.Strategy` to `ReturnResult`; for guards, use `GuardStrategy.ReturnResult`.
- **Package restore issues**: verify you are on .NET 8 or 10 and that your NuGet feed has access to `CleanArchitecture.Extensions.*` preview packages.
- **Slow handler warnings everywhere**: raise `PerformanceThresholdMilliseconds` or filter specific requests in your logging configuration.

## Where to go next

- Deep dives: [Core](../extensions/core.md) · [Result primitives](../extensions/core-result-primitives.md) · [Pipeline behaviors](../extensions/core-pipeline-behaviors.md) · [Validation](../extensions/validation.md)
- Concepts: [Architecture fit](../concepts/architecture-fit.md) · [Composition & invariants](../concepts/composition.md)
- Recipes: [Authentication](../recipes/authentication.md) · [Caching](../recipes/caching.md)
- Samples: [Samples index](../samples/index.md) (run any with `dotnet run --project <path>.csproj`)
- Roadmap and releases: [Roadmap](../roadmap.md) · [Release notes](../release-notes/index.md)

## Quick FAQ

- **Do I need to fork the template?** No. Install packages and register behaviors; the template stays pristine.
- **Can I remove the extensions later?** Yes. Uninstall the package and remove DI registrations to revert to baseline behavior.
- **What frameworks are supported?** Shipped packages target `net10.0`.
- **How do I migrate from the template's Result?** Use `LegacyResult`/`LegacyResult<T>` in Core and move handlers over gradually.
- **Is there a Minimal API/MVC adapter?** Planned for Validation; watch the roadmap for adapter releases.

## One-liner recap

Install Core, wire correlation/logging/performance behaviors, optionally add Validation with your preferred strategy, run a sample to verify correlation and validation logs, then move to the deep dives and recipes as you adopt more capabilities.
