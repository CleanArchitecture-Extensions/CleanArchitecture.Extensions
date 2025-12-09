# CleanArchitecture.Extensions

[![Docs](https://github.com/CleanArchitecture-Extensions/CleanArchitecture.Extensions/actions/workflows/docs.yml/badge.svg?branch=main)](https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions)
[![CodeQL](https://github.com/CleanArchitecture-Extensions/CleanArchitecture.Extensions/actions/workflows/codeql.yml/badge.svg?branch=main)](https://github.com/CleanArchitecture-Extensions/CleanArchitecture.Extensions/actions/workflows/codeql.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Built for developers who start from [Jason Taylor's Clean Architecture template](https://github.com/jasontaylordev/CleanArchitecture) and refuse to fork it. CleanArchitecture.Extensions is an ecosystem of opt-in NuGet packages that plug into the template without modifying the upstream repo. We keep the original pristine, add opinionated capabilities in layers, and publish a catalog of extensions you can compose. This README is intentionally extensive so that newcomers see the full vision in one place while experts can dive straight into the right sample or doc page.

Quick links:

- Quickstart: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/getting-started/quickstart/
- Extensions Catalog: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/extensions/
- Samples index: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/samples/
- Composition guidance: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/concepts/composition/
- Roadmap: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/roadmap/

## Table of contents

1. Why this exists and how it honors Jason's template
2. Status at a glance (shipped vs work in progress)
3. Getting started in ten minutes
4. Implemented packages (Core, Validation, Exceptions) with deep links
5. Extended deep dives for shipped packages
6. Full ecosystem map (summary)
7. Documentation map
8. Samples and runnable stories
9. Architecture values and compatibility promises
10. Adoption playbooks (greenfield and brownfield)
11. Release, quality, and operational guardrails
12. Repo layout and contribution pointers
13. FAQ
14. Appendix A (package-by-package blueprint)
15. Appendix B (scenario matrices)
16. Inspiration and next steps

## Why this project exists (and why Jason Taylor is in the header)

- The upstream template by Jason Taylor is the de facto starting point for many .NET teams. We link to it front and center because everything here is designed to sit beside it, not replace it.
- We do not fork the template. Instead, we ship extensions as NuGet packages that you can adopt gradually. The template stays clean; you stay unblocked.
- Each extension mirrors the style, folder layout, and dependency discipline of the template. Your team should feel zero friction moving between template code and extensions code.
- The docs, samples, and behaviors in this repo are structured to be understandable by anyone who has read Jason's README. If you know the template pipeline order, you will know exactly where to plug each extension.
- We want your first visit to this repo to be memorable: you see the full ecosystem vision, the concrete packages you can install today, and the detailed docs for everything else that is brewing.

## Status at a glance

- Implemented today (preview): `CleanArchitecture.Extensions.Core`, `CleanArchitecture.Extensions.Validation`, `CleanArchitecture.Extensions.Exceptions`
- In design/build-out (documented as work in progress): all remaining packages listed in the roadmap below (Caching, Multitenancy family, Enterprise extensions, SaaS extensions, Infrastructure adapters, Developer Experience toolchain).
- Target frameworks for shipped packages: `net8.0` and `net10.0`
- Packaging: SourceLink, XML docs, and snupkg symbols published with each package for debugger-friendly consumption.
- Documentation: MkDocs-powered site published to GitHub Pages; repo contains all source markdown under `docs/`.
- Samples: runnable sample projects live under `samples/`, with per-feature coverage for the implemented packages.
- Solution: open `CleanArchitecture.Extensions.sln` to see `src`, `tests`, `samples`, and `build` solution folders.

## Getting started in ten minutes

1. Install the template from Jason Taylor if you have not already: `dotnet new install Clean.Architecture.Solution.Template`. Clone the original repo if you want to compare patterns: https://github.com/jasontaylordev/CleanArchitecture.
2. Add our Core extension to your Application project:
   ```powershell
   dotnet add package CleanArchitecture.Extensions.Core --version 0.1.1-preview.1
   ```
   Register the behaviors you want:
   ```csharp
   services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CorrelationBehavior<,>));
   services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
   services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
   ```
3. Add Validation if you rely on FluentValidation in the pipeline:
   ```powershell
   dotnet add package CleanArchitecture.Extensions.Validation --version 0.1.1-preview.1
   ```
   Register validators and the behavior:
   ```csharp
   services.AddValidatorsFromAssemblyContaining<CreateOrderValidator>();
   services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
   ```
4. Run the sample projects to see the behaviors in action. The samples are all under `CleanArchitecture.Extensions/samples/`; pick the scenario that matches what you need (pipeline, logging, guards, results, time, options, domain events).
5. Read the documentation pages that match the features you turned on:
   - Core overview: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/extensions/core/
   - Result primitives: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/extensions/core-result-primitives/
   - Pipeline behaviors: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/extensions/core-pipeline-behaviors/
   - Validation: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/extensions/validation/
6. Decide what to adopt next. The roadmap is below; every package is opt-in and designed to be composable.

## Implemented packages (ready to use)

### CleanArchitecture.Extensions.Core — foundation primitives

Status: implemented (preview). Focus: give you Result, guard clauses, logging abstractions, correlation-aware pipeline behaviors, domain events, and deterministic time without forcing you off the template. Everything is dependency-light and matches the template's MediatR pipeline patterns.

What it solves:

- Rich `Result` and `Result<T>` types that keep trace IDs and error metadata instead of string arrays.
- Guard clauses to keep handlers slim while protecting invariants.
- Logging, correlation, and performance pipeline behaviors that respect the template's ordering.
- Domain event helpers that integrate with MediatR notifications.
- Time abstractions (`IClock`, `FrozenClock`, `OffsetClock`) that keep tests deterministic.
- Options toggles to configure behaviors without changing handler code.

Docs and navigation:

- Core overview: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/extensions/core/ (source: `docs/extensions/core.md`)
- Result primitives: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/extensions/core-result-primitives/ (source: `docs/extensions/core-result-primitives.md`)
- Guard clauses: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/extensions/core-guard-clauses/ (source: `docs/extensions/core-guard-clauses.md`)
- Pipeline behaviors: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/extensions/core-pipeline-behaviors/ (source: `docs/extensions/core-pipeline-behaviors.md`)
- Logging abstractions: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/extensions/core-logging-abstractions/ (source: `docs/extensions/core-logging-abstractions.md`)
- Domain events: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/extensions/core-domain-events/ (source: `docs/extensions/core-domain-events.md`)
- Time abstractions: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/extensions/core-time/ (source: `docs/extensions/core-time.md`)
- Options: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/extensions/core-options/ (source: `docs/extensions/core-options.md`)
- Package README: `src/CleanArchitecture.Extensions.Core/README.md`

Install:

```powershell
dotnet add package CleanArchitecture.Extensions.Core --version 0.1.1-preview.1
```

Samples you can run today (all under `CleanArchitecture.Extensions/samples/`):

- `CleanArchitecture.Extensions.Core.Pipeline.Sample`: shows correlation, logging, and performance behaviors wired in the pipeline.
- `CleanArchitecture.Extensions.Core.Logging.Sample`: demonstrates `IAppLogger`, logging scopes, and correlation metadata flowing through MediatR.
- `CleanArchitecture.Extensions.Core.Result.Sample`: exercises `Result`/`Result<T>`, legacy mapping (`LegacyResult`/`LegacyResult<T>`), and error aggregation.
- `CleanArchitecture.Extensions.Core.Guards.Sample`: explores guard clauses for null/empty, ranges, enums, and Result-aware guard strategies.
- `CleanArchitecture.Extensions.Core.DomainEvents.Sample`: walks through raising domain events on aggregates and dispatching them via MediatR.
- `CleanArchitecture.Extensions.Core.Time.Sample`: demonstrates `IClock`, `FrozenClock`, and `OffsetClock` to make time-dependent logic testable.
- `CleanArchitecture.Extensions.Core.Options.Sample`: shows how to toggle `CoreExtensionsOptions` for correlation IDs, thresholds, and guard strategies.

Where to look in tests:

- Core result and legacy shims: `tests/CleanArchitecture.Extensions.Core.Tests/LegacyResultTests.cs`
- Additional coverage: `tests/CleanArchitecture.Extensions.Core.Tests/` (see repository tree for up-to-date files as we expand coverage).

Guidance for template alignment:

- Pipeline order matches Jason's template: Correlation -> Logging pre-processor -> UnhandledException -> Authorization -> Validation -> Performance -> Handler.
- Legacy compatibility is built-in: use `LegacyResult.FromResult` and `LegacyResult<T>` to bridge from template code to the richer Result without rewriting everything at once.
- Logging abstractions (`IAppLogger`, `ILogContext`) are thin wrappers so you can back them with `Microsoft.Extensions.Logging` or Serilog; no vendor lock-in.

Scenarios that work out of the box:

- Build correlation-aware request logs without touching your handlers.
- Swap `DateTime.UtcNow` calls with `IClock.UtcNow` and freeze time in tests.
- Guard inputs consistently without copy/pasting null checks or error messages.
- Emit warnings for slow handlers using `PerformanceBehavior` with a configurable threshold.
- Raise and drain domain events while keeping Application free of infrastructure specifics.

### CleanArchitecture.Extensions.Validation — FluentValidation without template friction

Status: implemented (preview). Focus: predictable validation in the MediatR pipeline with FluentValidation, tuned for CleanArchitecture.Extensions.Core results and correlation IDs.

What it solves:

- Runs validators early in the pipeline and lets you choose the strategy: throw exception, return `Result`, or publish notifications.
- Correlation-aware logging of validation failures when `ILogContext`/`IAppLogger` are present.
- Shared rule catalog for common validation tasks (email, phone, pagination, culture, sort expression).
- Plugs into template's existing DI scanning and MediatR registration without changing handler signatures.

Docs and navigation:

- Validation overview: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/extensions/validation/ (source: `docs/extensions/validation.md`)
- Core interplay notes live in the Core docs sections linked above.
- Package README: `src/CleanArchitecture.Extensions.Validation/README.md`

Install:

```powershell
dotnet add package CleanArchitecture.Extensions.Validation --version 0.1.1-preview.1
```

Usage sketch:

```csharp
services.AddValidatorsFromAssemblyContaining<CreateOrderValidator>();
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

services.Configure<ValidationOptions>(options =>
{
    options.Strategy = ValidationStrategy.ReturnResult;
    options.IncludePropertyName = true;
    options.SeverityLogLevels[Severity.Error] = LogLevel.Warning;
    options.LogValidationFailures = true;
});
```

What you get when you adopt it:

- Validation behavior that respects the template order and plays well with `UnhandledExceptionBehaviour` and `AuthorizationBehaviour`.
- Tunable fail-fast vs collect-all semantics; cap maximum errors; pick whether to include attempted values and property names.
- Notification publishing hook (`IValidationNotificationPublisher`) if you need to surface validation feedback without throwing.
- Trace ID propagation that aligns with Core's correlation story; validation errors carry metadata suitable for localization downstream.

Planned next steps for Validation:

- Tenant-aware rules to align with the Multitenancy modules once they land.
- Minimal API and MVC filters as separate adapters so the same validators power both MediatR and HTTP endpoints.
- Metrics hooks for total validations and duration once the Observability module is online.

### CleanArchitecture.Extensions.Exceptions — predictable exception handling

Status: implemented (preview). Focus: catalog-driven exception mapping, MediatR wrapping behavior, base exception hierarchy, and redaction that keeps correlation/trace metadata intact.

What it solves:

- Ships base types (`NotFoundException`, `ConflictException`, `ForbiddenException`, `UnauthorizedException`, `TransientException`, etc.) plus retry-aware classification helpers.
- Exception catalog with stable codes, default messages, HTTP hints, and pluggable descriptors for consumer-specific codes.
- Pipeline behavior (`ExceptionWrappingBehavior`) that logs with correlation IDs, applies redaction, and can translate exceptions into `Result`/template-style Result without changing handler signatures.

Docs and navigation:

- Package README: `src/CleanArchitecture.Extensions.Exceptions/README.md`
- Design blueprint: `HighLevelDocs/Domain1-CoreArchitectureExtensions/CleanArchitecture.Extensions.Exceptions.md`

Install:

```powershell
dotnet add package CleanArchitecture.Extensions.Exceptions --version 0.1.1-preview.1
```

What you get when you adopt it:

- Consistent exception codes and messages across transports, with optional HTTP status metadata.
- Redaction of sensitive data before logging or surfacing messages.
- Drop-in MediatR pipeline registration aligned with the template’s unhandled exception slot.

## Extended deep dives for shipped packages

### Core component tour (Result, Guards, Behaviors, Logging, Domain Events, Time, Options)

- Result primitives: `Result` and `Result<T>` include status, error codes, messages, trace identifiers, and combinators such as `Map`, `Bind`, `Tap`, `Ensure`, `Recover`, and `Combine`. Legacy shims help you migrate from the template without rewriting contracts. See `docs/extensions/core-result-primitives.md` for examples that show mapping between the template's `Result` shape and the richer model.
- Guard clauses: extension methods keep handlers clean and consistent. Guards can return `Result` failures or throw depending on configuration. Catalog includes null/empty checks, range guards, enum guards, length validation, and state invariants. See `docs/extensions/core-guard-clauses.md` for patterns and error code guidance.
- Pipeline behaviors: correlation, logging, and performance behaviors align with the template order. Correlation assigns or honors correlation IDs; logging emits structured start/end entries; performance warns on slow handlers. See `docs/extensions/core-pipeline-behaviors.md` for ordering guidance and configuration tips.
- Logging abstractions: `IAppLogger` and `ILogContext` are minimal wrappers over your chosen logging stack. They provide structured scopes, correlation propagation, and a consistent API across the extensions family. See `docs/extensions/core-logging-abstractions.md` for adapter examples.
- Domain events: base `DomainEvent`, dispatcher abstraction, and aggregate helpers make it easy to raise events from entities and drain them later. The docs show how to integrate with MediatR notifications and how to clear events after dispatch.
- Time abstractions: `IClock` exposes time and identity helpers; `FrozenClock` enables deterministic tests; `OffsetClock` supports time travel scenarios. See `docs/extensions/core-time.md` for usage in handlers and interceptors.
- Options: `CoreExtensionsOptions` toggle defaults for correlation headers, ID factories, performance thresholds, guard strategies, and trace IDs. See `docs/extensions/core-options.md` for configuration examples and recommended defaults.

### Validation component tour (behaviors, strategies, rule catalog, logging integration)

- Behavior: `ValidationBehaviour<TRequest, TResponse>` runs validators discovered via DI, aggregates failures, and either throws, returns `Result`, or publishes notifications based on `ValidationOptions.Strategy`.
- Strategies: `Throw` mirrors the template's default behavior for web APIs; `ReturnResult` keeps handlers pure when you prefer explicit results; `Notify` triggers `IValidationNotificationPublisher` so downstream layers can respond without exceptions.
- Rule catalog: `CommonRules` holds reusable validators such as `NotEmptyTrimmed`, `EmailAddressBasic`, `OptionalEmailAddress`, `PositiveId`, `PageNumber`, `PageSize`, `PhoneE164`, `UrlAbsoluteHttpHttps`, `CultureCode`, `SortExpression`. These keep validators concise and consistent.
- Logging: integrates with `IAppLogger` and `ILogContext` to log validation summaries with correlation IDs. `ValidationOptions.SeverityLogLevels` lets you map validation severities to log levels.
- Planned additions: tenant-aware rules, minimal API filters, MVC filters, metrics, and notification adapters for UI surfaces.

## Full ecosystem map (summary)

We are building a plug-in ecosystem in six domains. Core, Validation, and Exceptions are shipping today; everything else is a documented work in progress. Each item below links to the canonical design notes in `HighLevelDocs/` so contributors can see intent and boundaries before writing code.

- Domain 1 (Core Architecture): Core, Validation, Exceptions, Caching.
- Domain 2 (Multitenancy): Multitenancy core plus EFCore, AspNetCore, Identity, Sharding, Provisioning, Storage, Redis adapters.
- Domain 3 (Enterprise): Audit, Settings, FeatureFlags, Notifications, RateLimiting, Localization, Authorization.
- Domain 4 (SaaS): Payments, Documents, UserManagement.
- Domain 5 (Infrastructure adapters): Redis, MessageBus, Observability, Storage, Search.
- Domain 6 (Developer Experience): CLI, Templates, Testing, SourceLinkAndSymbols, NuGetPackaging.

## Documentation map

- Home page: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/ (source: `docs/index.md`)
- Quickstart: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/getting-started/quickstart/ (source: `docs/getting-started/quickstart.md`)
- Installation: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/getting-started/installation/ (source: `docs/getting-started/installation.md`)
- Concepts (architecture fit): https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/concepts/architecture-fit/ (source: `docs/concepts/architecture-fit.md`)
- Concepts (composition and invariants): https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/concepts/composition/ (source: `docs/concepts/composition.md`)
- Extensions catalog landing: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/extensions/ (source: `docs/extensions/index.md`)
- Core extension deep dives: see links under the Core section above for each subpage.
- Validation deep dive: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/extensions/validation/ (source: `docs/extensions/validation.md`)
- Multitenancy core placeholder (design notes now, implementation pending): https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/extensions/multitenancy-core/ (source: `docs/extensions/multitenancy-core.md`)
- Recipes: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/recipes/authentication/ and https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/recipes/caching/ (sources under `docs/recipes/`)
- Samples overview: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/samples/ (source: `docs/samples/index.md`)
- Reference configuration: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/reference/configuration/ (source: `docs/reference/configuration.md`)
- Troubleshooting: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/troubleshooting/ (source: `docs/troubleshooting/index.md`)
- Contributing: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/contributing/ (source: `docs/contributing/index.md`)
- Release notes: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/release-notes/ (source: `docs/release-notes/index.md`)
- Roadmap: https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/roadmap/ (source: `docs/roadmap.md`)

## Samples and runnable stories

Every implemented feature ships with a runnable scenario. We keep samples lean so you can map them directly to your own codebase.

- Pipeline behaviors sample: `samples/CleanArchitecture.Extensions.Core.Pipeline.Sample` — shows MediatR pipeline wiring with correlation, logging, and performance behaviors and demonstrates how trace IDs flow to logs.
- Logging abstractions sample: `samples/CleanArchitecture.Extensions.Core.Logging.Sample` — focuses on `IAppLogger` and `ILogContext`, showing structured scopes and correlation metadata propagation.
- Result primitives sample: `samples/CleanArchitecture.Extensions.Core.Result.Sample` — demonstrates composing success/failure, mapping to/from template-style results via `LegacyResult`, and using trace IDs.
- Guard clauses sample: `samples/CleanArchitecture.Extensions.Core.Guards.Sample` — demonstrates guard patterns that return `Result` failures instead of throwing, plus throw-based mode for hard invariants.
- Domain events sample: `samples/CleanArchitecture.Extensions.Core.DomainEvents.Sample` — shows how to raise domain events from aggregates, dispatch them via MediatR, and clear them post-dispatch.
- Time abstractions sample: `samples/CleanArchitecture.Extensions.Core.Time.Sample` — switches between `IClock` implementations to test time-sensitive logic deterministically.
- Options sample: `samples/CleanArchitecture.Extensions.Core.Options.Sample` — configures `CoreExtensionsOptions` to change correlation header names, performance thresholds, and guard strategies without touching handlers.

Running a sample:

```powershell
cd CleanArchitecture.Extensions/samples/CleanArchitecture.Extensions.Core.Result.Sample
dotnet run
```

Each sample is self-contained; read its README or Program.cs for context. Outputs include console logs and behavior-specific traces to illustrate correlation IDs, errors, and success paths.

## Architecture values and compatibility promises

- Template-first: everything aligns with Jason Taylor's Clean Architecture template structure, namespaces, and MediatR pipeline ordering. We avoid surprises when you adopt an extension.
- Opt-in composition: every package is independent; you can bring in Core alone or combine multiple extensions when they are available. No forced mega-package.
- Minimal dependencies: Core depends on MediatR and Microsoft.Extensions.Options. Validation leans on FluentValidation. Future adapters will isolate heavy dependencies.
- Deterministic testing: time abstractions, logging fakes, and result shapes are all testable. Samples include deterministic clock usage to encourage the same in your code.
- Traceability: correlation IDs and trace IDs are first-class in results, behaviors, and logs to make debugging production issues easier.
- Backward compatibility: legacy result shims reduce migration risk from the template's original `Result` shape. We will continue to ship adapters where they reduce friction.

## Adoption playbooks

Greenfield (new project starting from the template):

- Step 1: scaffold a new solution using Jason's template.
- Step 2: install `CleanArchitecture.Extensions.Core` and wire correlation, logging, and performance behaviors in the Application project.
- Step 3: add `CleanArchitecture.Extensions.Validation` and register validators. Decide whether you want exceptions or `Result`-based validation outcomes.
- Step 4: use guard clauses in Application handlers; use `Result` combinators to keep handlers concise.
- Step 5: add time abstractions and switch auditing code to `IClock` so tests stay deterministic.
- Step 6: explore samples to copy patterns for domain events and logging scopes.
- Step 7: monitor the roadmap for multitenancy or caching if your product needs them; you can adopt them later without reshaping your code.

Brownfield (existing project on Jason's template):

- Step 1: add `CleanArchitecture.Extensions.Core` and use `LegacyResult` to bridge existing handlers that return the template's `Result`.
- Step 2: introduce `PerformanceBehavior` and `LoggingBehavior` to gain telemetry without modifying handlers.
- Step 3: migrate hot paths to `Result` and guard clauses incrementally. Keep `LegacyResult` mapping while you refactor.
- Step 4: introduce validation behavior with `ReturnResult` strategy to avoid throwing; or keep exceptions if your API pipeline depends on them.
- Step 5: replace direct `DateTime.UtcNow` usage with `IClock` by using wrappers or adapter methods. Tests become deterministic immediately.
- Step 6: plan for multitenancy or caching by reviewing the design docs; structure your code to make adoption straightforward when the packages drop.

## Release, quality, and operational guardrails

- Docs pipeline (`.github/workflows/docs.yml`) publishes the MkDocs site to `gh-pages` on changes to main.
- CodeQL (`.github/workflows/codeql.yml`) runs on pushes, PRs, and a weekly schedule to keep the codebase hardened.
- NuGet package builds include SourceLink and snupkg symbols; debugger-friendly out of the box.
- Target frameworks are `net8.0` and `net10.0` for the shipped packages, ensuring modern language/runtime support.
- Tests accompany each feature. If you add a new primitive or behavior, add tests beside it in `tests/`.
- Documentation-driven: every feature should land with docs in `docs/` and a sample in `samples/`; README links depend on that discipline.

## Repo layout (what to open first)

- `src/` — extension packages targeting the solution (`CleanArchitecture.Extensions.sln`). Start here when you need to inspect code or package structure.
- `tests/` — unit and integration tests per extension. We expand coverage alongside new features; see Core tests today.
- `samples/` — runnable scenarios for every feature. These are small projects you can copy/paste from.
- `docs/` — MkDocs source for the published site. Every page in the nav lives here.
- `.github/` — organization-level config and workflows. CI for docs and CodeQL are defined here.
- `HighLevelDocs/` — internal design docs that drive implementation. Before contributing to a package, read the matching design file in this folder.
- `CleanArchitecture.Extensions.sln` — solution file with `src`, `tests`, `samples`, `build` solution folders so you can open everything in one place.

## FAQ

- Do I need to fork Jason Taylor's template? No. Install his template and add extensions via NuGet. Keep the upstream repo clean.
- Are the extensions safe to adopt incrementally? Yes. Core and Validation are independent packages. Future packages will follow the same pattern.
- Why preview versions? We want feedback from real projects before locking APIs. SourceLink and symbols are included to make debugging easy.
- Where do I find docs for a specific primitive? Start at the Extensions Catalog page (`docs/extensions/index.md`), then open the detailed page such as `core-result-primitives`, `core-guard-clauses`, or `validation`.
- What about multitenancy? The full multitenancy stack is documented in `HighLevelDocs/Domain2-Multitenancy/*` and on the roadmap; implementation is underway. The catalog already includes a placeholder page to describe intent.
- Can I use this in .NET 6 or 7? The packages target `net8.0` and `net10.0`. For earlier runtimes, keep an eye on future compatibility notes in release docs.
- How do I contribute? Read `docs/contributing/index.md`, open the related HighLevelDocs design, and align with solution structure. PRs should include docs and tests where applicable.
- How do I keep track of updates? Watch the repository and the GitHub Pages site; release notes live in `docs/release-notes/index.md`.
- Why no massive umbrella package? Composition control belongs to the consumer. Packages stay focused and optional to keep dependency graphs tidy.
- What logging stack should I use? Any. `IAppLogger` and `ILogContext` are thin wrappers; adapt them to `ILogger<T>`, Serilog, or OpenTelemetry-compatible sinks.

## Appendix A — package-by-package blueprint (moved)

The long-form package blueprints now live in `docs/roadmap/package-blueprints.md`. Use the links below to jump directly:

- [CleanArchitecture.Extensions.Core](docs/roadmap/package-blueprints.md#cleanarchitectureextensionscore)
- [CleanArchitecture.Extensions.Validation](docs/roadmap/package-blueprints.md#cleanarchitectureextensionsvalidation)
- [CleanArchitecture.Extensions.Exceptions](docs/roadmap/package-blueprints.md#cleanarchitectureextensionsexceptions)
- [CleanArchitecture.Extensions.Caching](docs/roadmap/package-blueprints.md#cleanarchitectureextensionscaching)
- [CleanArchitecture.Extensions.Multitenancy](docs/roadmap/package-blueprints.md#cleanarchitectureextensionsmultitenancy)
- [CleanArchitecture.Extensions.Multitenancy.EFCore](docs/roadmap/package-blueprints.md#cleanarchitectureextensionsmultitenancyefcore)
- [CleanArchitecture.Extensions.Multitenancy.AspNetCore](docs/roadmap/package-blueprints.md#cleanarchitectureextensionsmultitenancyaspnetcore)
- [CleanArchitecture.Extensions.Multitenancy.Identity](docs/roadmap/package-blueprints.md#cleanarchitectureextensionsmultitenancyidentity)
- [CleanArchitecture.Extensions.Multitenancy.Sharding](docs/roadmap/package-blueprints.md#cleanarchitectureextensionsmultitenancysharding)
- [CleanArchitecture.Extensions.Multitenancy.Provisioning](docs/roadmap/package-blueprints.md#cleanarchitectureextensionsmultitenancyprovisioning)
- [CleanArchitecture.Extensions.Multitenancy.Storage](docs/roadmap/package-blueprints.md#cleanarchitectureextensionsmultitenancystorage)
- [CleanArchitecture.Extensions.Multitenancy.Redis](docs/roadmap/package-blueprints.md#cleanarchitectureextensionsmultitenancyredis)
- [CleanArchitecture.Extensions.Audit](docs/roadmap/package-blueprints.md#cleanarchitectureextensionsaudit)
- [CleanArchitecture.Extensions.Settings](docs/roadmap/package-blueprints.md#cleanarchitectureextensionssettings)
- [CleanArchitecture.Extensions.FeatureFlags](docs/roadmap/package-blueprints.md#cleanarchitectureextensionsfeatureflags)
- [CleanArchitecture.Extensions.Notifications](docs/roadmap/package-blueprints.md#cleanarchitectureextensionsnotifications)
- [CleanArchitecture.Extensions.RateLimiting](docs/roadmap/package-blueprints.md#cleanarchitectureextensionsratelimiting)
- [CleanArchitecture.Extensions.Localization](docs/roadmap/package-blueprints.md#cleanarchitectureextensionslocalization)
- [CleanArchitecture.Extensions.Authorization](docs/roadmap/package-blueprints.md#cleanarchitectureextensionsauthorization)
- [CleanArchitecture.Extensions.Payments](docs/roadmap/package-blueprints.md#cleanarchitectureextensionspayments)
- [CleanArchitecture.Extensions.Documents](docs/roadmap/package-blueprints.md#cleanarchitectureextensionsdocuments)
- [CleanArchitecture.Extensions.UserManagement](docs/roadmap/package-blueprints.md#cleanarchitectureextensionsusermanagement)
- [CleanArchitecture.Extensions.Redis](docs/roadmap/package-blueprints.md#cleanarchitectureextensionsredis)
- [CleanArchitecture.Extensions.MessageBus](docs/roadmap/package-blueprints.md#cleanarchitectureextensionsmessagebus)
- [CleanArchitecture.Extensions.Observability](docs/roadmap/package-blueprints.md#cleanarchitectureextensionsobservability)
- [CleanArchitecture.Extensions.Storage](docs/roadmap/package-blueprints.md#cleanarchitectureextensionsstorage)
- [CleanArchitecture.Extensions.Search](docs/roadmap/package-blueprints.md#cleanarchitectureextensionssearch)
- [CleanArchitecture.Extensions.CLI](docs/roadmap/package-blueprints.md#cleanarchitectureextensionscli)
- [CleanArchitecture.Extensions.Templates](docs/roadmap/package-blueprints.md#cleanarchitectureextensionstemplates)
- [CleanArchitecture.Extensions.Testing](docs/roadmap/package-blueprints.md#cleanarchitectureextensionstesting)
- [CleanArchitecture.Extensions.SourceLinkAndSymbols](docs/roadmap/package-blueprints.md#cleanarchitectureextensionssourcelinkandsymbols)
- [CleanArchitecture.Extensions.NuGetPackaging](docs/roadmap/package-blueprints.md#cleanarchitectureextensionsnugetpackaging)

## Appendix B — scenario matrices and play-patterns (moved)

The scenario playbooks now live in `docs/getting-started/adoption-playbooks.md`. Jump to the one you need:

- [Telemetry-first adoption](docs/getting-started/adoption-playbooks.md#telemetry-first-adoption)
- [Migration from template Result](docs/getting-started/adoption-playbooks.md#migration-from-template-result)
- [SaaS with tenant isolation](docs/getting-started/adoption-playbooks.md#saas-with-tenant-isolation)
- [Event-driven integration](docs/getting-started/adoption-playbooks.md#event-driven-integration)
- [Compliance and audit readiness](docs/getting-started/adoption-playbooks.md#compliance-and-audit-readiness)
- [Developer experience at scale](docs/getting-started/adoption-playbooks.md#developer-experience-at-scale)



## Inspiration and gratitude

This project exists because of Jason Taylor's Clean Architecture template. The template gives teams a clear starting point; this repository gives them an ecosystem to grow without forking. If you are new here, start by reading Jason's repository: https://github.com/jasontaylordev/CleanArchitecture. Then come back and plug in only what you need.

## Next steps for readers

- If you want to build with what exists today: install Core and Validation, run the samples, read the Core and Validation docs, and wire the behaviors into your pipeline.
- If you want to contribute: pick a work-in-progress module, read its HighLevelDocs design, and start with docs and sample scaffolding before coding.
- If you want to keep tabs: watch the repo and the GitHub Pages site for release notes and roadmap updates.

---

This README is intentionally comprehensive and long-form to capture newcomers at first glance and give veterans the depth they need. The links above take you directly to the working docs, samples, and design plans. Use what you need today, and help shape what ships next.
