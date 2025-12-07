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
4. Implemented packages (Core, Validation) with deep links
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

- Implemented today (preview): `CleanArchitecture.Extensions.Core`, `CleanArchitecture.Extensions.Validation`
- In design/build-out (documented as work in progress): all remaining packages listed in the roadmap below (Exceptions, Caching, Multitenancy family, Enterprise extensions, SaaS extensions, Infrastructure adapters, Developer Experience toolchain).
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

We are building a plug-in ecosystem in six domains. Only Core and Validation are shipping today; everything else is a documented work in progress. Each item below links to the canonical design notes in `HighLevelDocs/` so contributors can see intent and boundaries before writing code.

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

## Appendix A — package-by-package blueprint (long form)

#### CleanArchitecture.Extensions.Core (shipped — Core Architecture)

What we are building:
CleanArchitecture.Extensions.Core is focused on Rich Result types, guard clauses, correlation-aware pipeline behaviors, logging abstractions, domain events, and deterministic time abstractions that mirror Jason Taylor's template without changing handler signatures. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Developers need drop-in behaviors for correlation, logging, and performance, plus expressive Result shapes to replace bool+errors without rewriting controllers. They also need guard clauses, domain events, and clocks to keep tests deterministic and handlers slim. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Plugs directly into MediatR pipelines and can be adopted incrementally using LegacyResult shims. Works with any logger through IAppLogger/ILogContext and feeds trace identifiers to downstream modules. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain1-CoreArchitectureExtensions/CleanArchitecture.Extensions.Core.md
- Planned docs entry: docs/extensions/core.md
- Sample plan: Samples already available for pipeline, logging, results, guards, time, options, and domain events.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Core would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Core will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Validation (shipped — Core Architecture)

What we are building:
CleanArchitecture.Extensions.Validation is focused on FluentValidation-powered MediatR behavior with switchable strategies (throw, return Result, notify) and shared rule catalog for email, phone, pagination, culture, and sort expressions. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Teams want predictable validation errors with correlation IDs, consistent logging, and the ability to avoid exceptions in background jobs while keeping the template's MediatR signature. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Aligns with Core's Result and logging abstractions, uses DI scanning from the template, and will later add minimal API filters and tenant-aware rules. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain1-CoreArchitectureExtensions/CleanArchitecture.Extensions.Validation.md
- Planned docs entry: docs/extensions/validation.md
- Sample plan: Sample ideas include validation in the pipeline plus notification publishers; shipped samples will accompany the package.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Validation would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Validation will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Exceptions (work in progress — Core Architecture)

What we are building:
CleanArchitecture.Extensions.Exceptions is focused on Exception translation and wrapping behaviors that convert domain/application exceptions to structured errors or Results while preserving correlation IDs. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Apps that need consistent problem details without leaking infrastructure exceptions, plus the ability to log failures with trace IDs and error codes that map to clients. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will sit after correlation/logging behaviors and before validation/performance in the pipeline; adapters will integrate with ASP.NET Core middleware. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain1-CoreArchitectureExtensions/CleanArchitecture.Extensions.Exceptions.md
- Planned docs entry: docs/extensions/exceptions.md (planned)
- Sample plan: Planned samples will demonstrate exception translation for HTTP endpoints and background jobs.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Exceptions would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Exceptions will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Caching (work in progress — Core Architecture)

What we are building:
CleanArchitecture.Extensions.Caching is focused on Cache abstractions with memory and distributed providers plus pipeline behaviors for caching queries and invalidating on commands. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Teams that need deterministic cache keys, tenant-aware scoping, and cache busting tied to domain events without sprinkling cache logic across handlers. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will layer on Core's logging, correlation, and Result primitives, with adapters for Redis and other stores arriving via Domain 5. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain1-CoreArchitectureExtensions/CleanArchitecture.Extensions.Caching.md
- Planned docs entry: docs/extensions/caching.md (planned)
- Sample plan: Samples will include query caching with eviction on command success and cache-hit telemetry.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Caching would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Caching will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Multitenancy (work in progress — Multitenancy)

What we are building:
CleanArchitecture.Extensions.Multitenancy is focused on Tenant info model, current tenant provider, header/host/route/claim/composite resolution, middleware, attributes, and enforcement/validation behaviors. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
SaaS teams need tenant resolution at HTTP edges, enforcement in MediatR, and tenant metadata flowing into logging and caching to avoid cross-tenant leakage. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will provide ICurrentTenant abstraction consumed by EFCore, Identity, caching, storage, and messaging adapters. Pipeline behaviors will align with validation and authorization ordering. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain2-Multitenancy/CleanArchitecture.Extensions.Multitenancy.md
- Planned docs entry: docs/extensions/multitenancy-core.md
- Sample plan: Planned samples include header-based and route-based tenant resolution with enforcement behaviors in action.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Multitenancy would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Multitenancy will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Multitenancy.EFCore (work in progress — Multitenancy)

What we are building:
CleanArchitecture.Extensions.Multitenancy.EFCore is focused on EF Core helpers for shared database, schema-per-tenant, and database-per-tenant strategies with global query filters and tenant-aware DbContext factories. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Projects need to enforce tenant isolation in EF, manage migrations per tenant, and seed data safely while switching strategies over time. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will hook into the multitenancy core for tenant context, reuse Core logging and trace IDs, and ship interceptors/filters to add tenant predicates automatically. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain2-Multitenancy/CleanArchitecture.Extensions.Multitenancy.EFCore.md
- Planned docs entry: docs/extensions/multitenancy-efcore.md (planned)
- Sample plan: Samples will show global filters, schema migrations, and seeding scripts for new tenants.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Multitenancy.EFCore would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Multitenancy.EFCore will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Multitenancy.AspNetCore (work in progress — Multitenancy)

What we are building:
CleanArchitecture.Extensions.Multitenancy.AspNetCore is focused on AspNetCore helpers for route/host/header tenant providers, minimal API filters, endpoint metadata, and middleware that sets tenant context early. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
APIs that must reject requests without tenant context and supply consistent error responses; apps that map tenants to routes or hosts and need helpers to stay DRY. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will align with the template's middleware pipeline, flow tenant info into ILogContext, and compose with Validation and Authorization behaviors. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain2-Multitenancy/CleanArchitecture.Extensions.Multitenancy.AspNetCore.md
- Planned docs entry: docs/extensions/multitenancy-aspnetcore.md (planned)
- Sample plan: Samples will demonstrate minimal API and controller setups with tenant filters and friendly error messages.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Multitenancy.AspNetCore would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Multitenancy.AspNetCore will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Multitenancy.Identity (work in progress — Multitenancy)

What we are building:
CleanArchitecture.Extensions.Multitenancy.Identity is focused on Tenant-aware Identity extensions including tenant claim mapping, ITenantUser abstractions, policy provider, and per-tenant role handling. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Authentication flows where users belong to specific tenants, login isolation is required, and tokens must carry tenant claims reliably. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will integrate with ASP.NET Core Identity, multitenancy core, and Authorization policies while remaining optional for non-Identity auth stacks. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain2-Multitenancy/CleanArchitecture.Extensions.Multitenancy.Identity.md
- Planned docs entry: docs/extensions/multitenancy-identity.md (planned)
- Sample plan: Samples will cover login flows that populate tenant claims and authorize tenant-scoped resources.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Multitenancy.Identity would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Multitenancy.Identity will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Multitenancy.Sharding (work in progress — Multitenancy)

What we are building:
CleanArchitecture.Extensions.Multitenancy.Sharding is focused on Shard resolution strategies (hash, range, geo) and connection managers for large datasets that need partitioning alongside tenant isolation. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Data-intensive systems that must route requests to the right shard while keeping tenant context intact; operators who need to rebalance shards safely. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will align with multitenancy core for tenant identification, with caching and message bus adapters to keep shard context consistent end to end. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain2-Multitenancy/CleanArchitecture.Extensions.Multitenancy.Sharding.md
- Planned docs entry: docs/extensions/multitenancy-sharding.md (planned)
- Sample plan: Samples will show shard selection strategies and metrics for shard performance.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Multitenancy.Sharding would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Multitenancy.Sharding will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Multitenancy.Provisioning (work in progress — Multitenancy)

What we are building:
CleanArchitecture.Extensions.Multitenancy.Provisioning is focused on Tenant onboarding workflows, provisioning events, schema creation, and per-tenant seed orchestration. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Automating tenant creation, initializing schemas, and notifying other systems (billing, notifications) as tenants onboard or offboard. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will emit events that other modules (Notifications, Audit) can subscribe to, and will use storage/EF adapters to create per-tenant artifacts. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain2-Multitenancy/CleanArchitecture.Extensions.Multitenancy.Provisioning.md
- Planned docs entry: docs/extensions/multitenancy-provisioning.md (planned)
- Sample plan: Samples will walk through end-to-end tenant provisioning from HTTP request to seeded data.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Multitenancy.Provisioning would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Multitenancy.Provisioning will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Multitenancy.Storage (work in progress — Multitenancy)

What we are building:
CleanArchitecture.Extensions.Multitenancy.Storage is focused on Tenant-aware blob and file storage abstractions with folder strategies and consistent keying. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Products that need to segregate files per tenant on shared storage accounts, enforce folder naming, and migrate existing assets safely. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will leverage ICurrentTenant from multitenancy core and storage adapters from Domain 5 to keep physical storage aligned with tenant context. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain2-Multitenancy/CleanArchitecture.Extensions.Multitenancy.Storage.md
- Planned docs entry: docs/extensions/multitenancy-storage.md (planned)
- Sample plan: Samples will include upload/download flows that automatically scope paths by tenant.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Multitenancy.Storage would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Multitenancy.Storage will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Multitenancy.Redis (work in progress — Multitenancy)

What we are building:
CleanArchitecture.Extensions.Multitenancy.Redis is focused on Tenant-safe Redis usage with key prefixing conventions and helpers for cache isolation. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Distributed caches shared across tenants that require strict key isolation and predictable eviction strategies. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will sit on top of the Caching module and coordinate with multitenancy core to derive tenant-aware cache keys. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain2-Multitenancy/CleanArchitecture.Extensions.Multitenancy.Redis.md
- Planned docs entry: docs/extensions/multitenancy-redis.md (planned)
- Sample plan: Samples will show cache gets/sets with tenant prefixes and metrics for cache hits by tenant.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Multitenancy.Redis would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Multitenancy.Redis will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Audit (work in progress — Enterprise)

What we are building:
CleanArchitecture.Extensions.Audit is focused on Audit events, entity diffs, command audit behaviors, and multiple audit stores (SQL, file, bus). This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Teams that need compliance-ready audit logs tied to correlation IDs and entity changes, with storage choices based on environment. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will depend on Core for time and correlation, integrate with multitenancy for tenant markers, and expose behaviors that plug into MediatR. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain3-EnterpriseExtensions/CleanArchitecture.Extensions.Audit.md
- Planned docs entry: docs/extensions/audit.md (planned)
- Sample plan: Samples will illustrate audit events for commands and entity diffs with storage providers swapped in/out.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Audit would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Audit will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Settings (work in progress — Enterprise)

What we are building:
CleanArchitecture.Extensions.Settings is focused on Runtime settings with key-value and JSON providers, typed accessors, and optional tenant overrides without redeployments. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Operations teams adjusting thresholds, toggles, or URLs at runtime; product teams enabling tenant-specific overrides without code changes. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will align with Options pattern, integrate with FeatureFlags and Authorization, and support database/file-backed providers. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain3-EnterpriseExtensions/CleanArchitecture.Extensions.Settings.md
- Planned docs entry: docs/extensions/settings.md (planned)
- Sample plan: Samples will include dynamic configuration reload and tenant override examples.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Settings would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Settings will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.FeatureFlags (work in progress — Enterprise)

What we are building:
CleanArchitecture.Extensions.FeatureFlags is focused on Feature definitions, providers (memory/db), tenant/user overrides, evaluation behavior, and feature attributes for handlers and endpoints. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Progressive rollout of features, canary releases per tenant, and experimentation without long-lived branches. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will integrate with Settings and Authorization, use correlation IDs for observability, and expose MediatR behaviors for feature checks. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain3-EnterpriseExtensions/CleanArchitecture.Extensions.FeatureFlags.md
- Planned docs entry: docs/extensions/feature-flags.md (planned)
- Sample plan: Samples will demonstrate feature evaluation in commands and minimal APIs with toggle switches.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.FeatureFlags would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.FeatureFlags will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Notifications (work in progress — Enterprise)

What we are building:
CleanArchitecture.Extensions.Notifications is focused on Unified notification pipeline with providers for email, SMS, push, and template management. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Send templated messages in response to domain events or application commands; switch providers without changing handler logic. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will plug into Core logging and multitenancy for tenant-specific channels, and expose a NotificationBehavior for MediatR when needed. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain3-EnterpriseExtensions/CleanArchitecture.Extensions.Notifications.md
- Planned docs entry: docs/extensions/notifications.md (planned)
- Sample plan: Samples will show email and SMS dispatch using a template engine and mock providers for tests.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Notifications would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Notifications will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.RateLimiting (work in progress — Enterprise)

What we are building:
CleanArchitecture.Extensions.RateLimiting is focused on Configurable rate limits for commands, queries, and APIs with tenant/IP/user scopes and friendly failure surfaces. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Protect APIs from abuse, enforce tenant-specific quotas, and return consistent error payloads when limits are exceeded. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will offer minimal API filters, middleware, and MediatR behaviors; will integrate with Observability for metrics and with Multitenancy for tenant-aware limits. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain3-EnterpriseExtensions/CleanArchitecture.Extensions.RateLimiting.md
- Planned docs entry: docs/extensions/rate-limiting.md (planned)
- Sample plan: Samples will cover per-tenant and per-IP throttling with metrics emitted for dashboards.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.RateLimiting would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.RateLimiting will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Localization (work in progress — Enterprise)

What we are building:
CleanArchitecture.Extensions.Localization is focused on Resource providers, fallback rules, and dynamic translation helpers with optional tenant scoping. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Applications targeting multiple locales needing resource lookups that can be overridden per tenant or environment without code changes. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will align with Validation and Exceptions to localize messages and with multitenancy for per-tenant resource sets. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain3-EnterpriseExtensions/CleanArchitecture.Extensions.Localization.md
- Planned docs entry: docs/extensions/localization.md (planned)
- Sample plan: Samples will demonstrate localized validation errors and exception messages across cultures.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Localization would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Localization will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Authorization (work in progress — Enterprise)

What we are building:
CleanArchitecture.Extensions.Authorization is focused on Permission model, policy providers, role-per-tenant support, and decision engine for commands/queries and HTTP endpoints. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Permission-aware handlers that need to evaluate complex policies, sometimes per tenant, and return consistent failure responses. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will integrate with Identity when present, reuse multitenancy context, and expose behaviors to run before Validation/Performance. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain3-EnterpriseExtensions/CleanArchitecture.Extensions.Authorization.md
- Planned docs entry: docs/extensions/authorization.md (planned)
- Sample plan: Samples will cover permission checks in handlers and minimal APIs with policy wiring.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Authorization would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Authorization will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Payments (work in progress — SaaS)

What we are building:
CleanArchitecture.Extensions.Payments is focused on Payments and subscriptions (starting with Stripe), webhook processing, billing events, and subscription lifecycle helpers. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
SaaS products billing tenants, handling upgrades/downgrades, and reacting to webhooks without scattering billing logic across handlers. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will publish billing events that Notifications and Provisioning can consume, and will include adapters for multitenant billing contexts. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain4-SaaSBusinessExtensions/CleanArchitecture.Extensions.Payments.md
- Planned docs entry: docs/extensions/payments.md (planned)
- Sample plan: Samples will mock Stripe flows, webhook verification, and subscription state transitions.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Payments would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Payments will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Documents (work in progress — SaaS)

What we are building:
CleanArchitecture.Extensions.Documents is focused on Document generation for PDF/Excel with template-driven rendering and storage-friendly outputs. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Exporting invoices, reports, and data extracts while keeping generation out of handlers and aligned with storage abstractions. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will work with Storage adapters to persist artifacts, with Notifications to deliver links, and with Settings for template configuration. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain4-SaaSBusinessExtensions/CleanArchitecture.Extensions.Documents.md
- Planned docs entry: docs/extensions/documents.md (planned)
- Sample plan: Samples will include report generation with template engines and storage uploads.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Documents would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Documents will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.UserManagement (work in progress — SaaS)

What we are building:
CleanArchitecture.Extensions.UserManagement is focused on User onboarding, invitations, MFA abstraction, and passwordless flows tuned for multitenant applications. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Invite flows, role assignment per tenant, and MFA enforcement without embedding provider specifics into Application handlers. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will align with Identity and Authorization modules, expose events for Notifications, and respect multitenancy context. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain4-SaaSBusinessExtensions/CleanArchitecture.Extensions.UserManagement.md
- Planned docs entry: docs/extensions/user-management.md (planned)
- Sample plan: Samples will demonstrate invitation acceptance, MFA setup, and passwordless login with mock providers.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.UserManagement would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.UserManagement will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Redis (work in progress — Infrastructure)

What we are building:
CleanArchitecture.Extensions.Redis is focused on Redis adapters for the caching abstraction with multi-tenant key strategies and optional pub/sub helpers. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Projects standardizing on Redis who want consistent key naming, instrumentation, and tenant-safe operations. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will wrap the Caching module APIs and align with Multitenancy.Redis for key prefixes and eviction rules. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain5-InfrastructureAdapters/CleanArchitecture.Extensions.Redis.md
- Planned docs entry: docs/extensions/redis.md (planned)
- Sample plan: Samples will show Redis-backed caching with health checks and diagnostics.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Redis would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Redis will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.MessageBus (work in progress — Infrastructure)

What we are building:
CleanArchitecture.Extensions.MessageBus is focused on Message bus adapters for RabbitMQ, Azure Service Bus, and Kafka, with outbox patterns and integration event pipelines. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Handlers publishing integration events that must carry correlation and tenant metadata; consumers needing retry/resiliency guidance. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will align with Core correlation/logging, integrate with Observability for tracing, and expose message handlers that mimic MediatR behaviors. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain5-InfrastructureAdapters/CleanArchitecture.Extensions.MessageBus.md
- Planned docs entry: docs/extensions/message-bus.md (planned)
- Sample plan: Samples will include outbox/inbox flows and multi-transport adapters.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.MessageBus would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.MessageBus will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Observability (work in progress — Infrastructure)

What we are building:
CleanArchitecture.Extensions.Observability is focused on OpenTelemetry tracing, logging enrichers, and metrics that honor correlation IDs and tenant context set by Core and Multitenancy. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Teams instrumenting handlers, message processing, and HTTP endpoints who want OTEL export with minimal wiring and consistent metadata. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will wrap Core logging abstractions, integrate with message bus adapters, and emit metrics that RateLimiting and Caching can use. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain5-InfrastructureAdapters/CleanArchitecture.Extensions.Observability.md
- Planned docs entry: docs/extensions/observability.md (planned)
- Sample plan: Samples will show trace propagation through MediatR, message buses, and HTTP requests.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Observability would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Observability will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Storage (work in progress — Infrastructure)

What we are building:
CleanArchitecture.Extensions.Storage is focused on Storage adapters for Azure Blob, AWS S3, and local storage with consistent abstractions and retry guidance. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Projects needing to swap storage providers without touching Application code, with optional tenant folder strategies from Multitenancy.Storage. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will expose a storage abstraction consumed by Documents, UserManagement, and Multitenancy.Storage while using Core logging for diagnostics. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain5-InfrastructureAdapters/CleanArchitecture.Extensions.Storage.md
- Planned docs entry: docs/extensions/storage.md (planned)
- Sample plan: Samples will demonstrate uploads/downloads with different providers and integration with tenant folder strategies.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Storage would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Storage will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Search (work in progress — Infrastructure)

What we are building:
CleanArchitecture.Extensions.Search is focused on Search adapters for ElasticSearch, Azure Search, and full-text providers with clean query interfaces. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Applications implementing search without committing to a provider, needing multi-tenant index strategies and consistent analyzers. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will align with Observability and Multitenancy to propagate correlation and tenant metadata; may include pipelines for indexing domain events. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain5-InfrastructureAdapters/CleanArchitecture.Extensions.Search.md
- Planned docs entry: docs/extensions/search.md (planned)
- Sample plan: Samples will show indexing and querying with interchangeable providers and tenant-aware indexes.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Search would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Search will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.CLI (work in progress — Developer Experience)

What we are building:
CleanArchitecture.Extensions.CLI is focused on CLI for scaffolding modules, wiring extensions into projects, and generating CQRS artifacts with consistent conventions. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Developers who want `caext new module` or `caext add multitenancy` commands to wire packages and samples quickly. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will rely on Templates and NuGetPackaging conventions, and will include commands to fetch latest docs or snippets. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain6-DeveloperExperience/CleanArchitecture.Extensions.CLI.md
- Planned docs entry: docs/extensions/cli.md (planned)
- Sample plan: Samples will be command transcripts and generated projects showing the CLI output.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.CLI would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.CLI will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Templates (work in progress — Developer Experience)

What we are building:
CleanArchitecture.Extensions.Templates is focused on Template variants layered on Jason's base: multitenant, audit-ready, SaaS, and microservices-friendly starters. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Teams that want prewired solutions with extensions included, ensuring composition rules are respected without manual steps. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will track the upstream template closely, integrate with CLI commands, and include sample data and scripts per variant. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain6-DeveloperExperience/CleanArchitecture.Extensions.Templates.md
- Planned docs entry: docs/extensions/templates.md (planned)
- Sample plan: Templates will include runnable starter solutions; docs will show what is wired where.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Templates would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Templates will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.Testing (work in progress — Developer Experience)

What we are building:
CleanArchitecture.Extensions.Testing is focused on Testing utilities for CQRS handlers, tenant-aware contexts, and pipeline behaviors with fakes and fixtures. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Developers wanting to assert Results, domain events, and tenant-aware behaviors without verbose setup code. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will reuse Core abstractions (clock, logger, results) and plug into multitenancy/testing helpers for tenant context. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain6-DeveloperExperience/CleanArchitecture.Extensions.Testing.md
- Planned docs entry: docs/extensions/testing.md (planned)
- Sample plan: Samples will include test fixtures for handlers and pipeline behaviors with deterministic clocks.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.Testing would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.Testing will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.SourceLinkAndSymbols (work in progress — Developer Experience)

What we are building:
CleanArchitecture.Extensions.SourceLinkAndSymbols is focused on Build-time helpers to ensure NuGet packages ship with SourceLink and snupkg symbols consistently across modules. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Package maintainers ensuring debugger-friendly distributions without copy/pasting configuration across projects. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will be consumed by the build pipeline and referenced by new modules via props/targets files. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain6-DeveloperExperience/CleanArchitecture.Extensions.SourceLinkAndSymbols.md
- Planned docs entry: docs/extensions/sourcelink-and-symbols.md (planned)
- Sample plan: Samples will demonstrate stepping into package source from a consumer project.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.SourceLinkAndSymbols would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.SourceLinkAndSymbols will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

#### CleanArchitecture.Extensions.NuGetPackaging (work in progress — Developer Experience)

What we are building:
CleanArchitecture.Extensions.NuGetPackaging is focused on Packaging conventions, semantic versioning guidance, and CI tasks for publishing extensions with consistent metadata. This extension exists to keep the Clean Architecture layers honest while reducing the friction that teams feel when wiring cross-cutting concerns by hand. The goal is to let you add capability without bending the original template or leaking infrastructure details into Application and Domain code.

Why it matters for Clean Architecture teams:
Maintainers who need repeatable packaging with version bumps, changelog updates, and artifact signing across the ecosystem. The package narrative lives in the roadmap because we want contributors to see the intended surface area before a single line of code is written. When this lands, expect to see configuration defaults that match the template, clear points to hook into MediatR or middleware, and a way to back out gracefully if you choose not to adopt it.

How it will integrate:
Will be consumed by build scripts and CLI templates, ensuring every module ships with the same quality gates. We will reuse correlation IDs, logging scopes, and Result metadata from Core wherever possible so diagnostics stay consistent. Each adapter or behavior will be optional; you choose how deep you want to go based on your deployment and compliance needs.

Documentation and design trail:

- Design notes: HighLevelDocs/Domain6-DeveloperExperience/CleanArchitecture.Extensions.NuGetPackaging.md
- Planned docs entry: docs/extensions/nuget-packaging.md (planned)
- Sample plan: Samples will include CI snippets and local packaging walkthroughs.

Adoption guidance:
Expect a phased rollout: design doc review, doc stubs under `docs/extensions/`, sample scaffolding under `samples/`, then code with tests. Early adopters can start by reading the HighLevelDocs entry and sketching how CleanArchitecture.Extensions.NuGetPackaging would plug into their pipelines. We will ship SourceLink, XML docs, symbols, and a README with concrete install/usage guidance just like Core and Validation.

Interoperability patterns:
CleanArchitecture.Extensions.NuGetPackaging will be deliberate about compatibility with other extensions. We will highlight safe ordering in the MediatR pipeline, how to mix with multitenancy (when applicable), and what to do when pairing with Observability or Caching. For teams migrating from the baseline template, we will document shims or adapters so you do not have to rewrite working code to adopt this module.

Monitoring and reliability:
Behaviors and adapters will emit correlation-aware logs and, when relevant, metrics that feed Observability once it ships. Failure modes will be documented with mitigations and configuration toggles. Expect guidance on defaults vs production hardening, plus testing tips using fakes or in-memory providers.

## Appendix B — scenario matrices and play-patterns

### Telemetry-first adoption

Start with Core correlation, logging, and performance behaviors to light up request tracing in development. Layer Observability (when available) to export OTEL traces without touching handlers. Pair with Validation to surface meaningful errors early. This pattern suits teams that need better diagnostics before expanding functionality.

How to get started:

- Identify which extensions are already available (Core and Validation today) and wire them following the quickstart.
- Read the matching HighLevelDocs entries for upcoming modules to understand required seams in your code.
- Sketch pipeline ordering and configuration toggles so adoption is a config change, not a refactor.
- Run the samples closest to your scenario and adapt them into your solution structure.
- Track the docs site and release notes for module-specific guidance as packages ship.

Success signals to look for:

- Logs and traces include correlation IDs and, when applicable, tenant identifiers.
- Validation errors are predictable and localized to the entry points where they belong.
- Pipeline ordering is explicit and documented in your solution.
- Teams can add or remove an extension via NuGet plus configuration without code churn.
- Samples and docs in this repo stay aligned with what you have in production.

### Migration from template Result

Use Core's LegacyResult shims to bridge from the template's bool+errors shape to the richer Result model. Migrate handlers incrementally while keeping controllers untouched. Add Validation with ReturnResult strategy to avoid exceptions where you want explicit outcomes. This keeps production stable while you modernize internals.

How to get started:

- Identify which extensions are already available (Core and Validation today) and wire them following the quickstart.
- Read the matching HighLevelDocs entries for upcoming modules to understand required seams in your code.
- Sketch pipeline ordering and configuration toggles so adoption is a config change, not a refactor.
- Run the samples closest to your scenario and adapt them into your solution structure.
- Track the docs site and release notes for module-specific guidance as packages ship.

Success signals to look for:

- Logs and traces include correlation IDs and, when applicable, tenant identifiers.
- Validation errors are predictable and localized to the entry points where they belong.
- Pipeline ordering is explicit and documented in your solution.
- Teams can add or remove an extension via NuGet plus configuration without code churn.
- Samples and docs in this repo stay aligned with what you have in production.

### SaaS with tenant isolation

Adopt multitenancy core for tenant resolution and enforcement once it lands. Pair it with EFCore and AspNetCore adapters to enforce tenant boundaries in data and HTTP layers. Add RateLimiting and Caching using tenant-aware strategies. When billing arrives, connect Payments and Provisioning to automate onboarding and subscription state.

How to get started:

- Identify which extensions are already available (Core and Validation today) and wire them following the quickstart.
- Read the matching HighLevelDocs entries for upcoming modules to understand required seams in your code.
- Sketch pipeline ordering and configuration toggles so adoption is a config change, not a refactor.
- Run the samples closest to your scenario and adapt them into your solution structure.
- Track the docs site and release notes for module-specific guidance as packages ship.

Success signals to look for:

- Logs and traces include correlation IDs and, when applicable, tenant identifiers.
- Validation errors are predictable and localized to the entry points where they belong.
- Pipeline ordering is explicit and documented in your solution.
- Teams can add or remove an extension via NuGet plus configuration without code churn.
- Samples and docs in this repo stay aligned with what you have in production.

### Event-driven integration

Lean on Core domain events today and plan for MessageBus and Outbox patterns to publish integration events with correlation IDs and tenant context. Observability will carry traces across process boundaries. Notifications can react to events for user-facing messages. This pattern is suited for teams integrating with other services via asynchronous pipelines.

How to get started:

- Identify which extensions are already available (Core and Validation today) and wire them following the quickstart.
- Read the matching HighLevelDocs entries for upcoming modules to understand required seams in your code.
- Sketch pipeline ordering and configuration toggles so adoption is a config change, not a refactor.
- Run the samples closest to your scenario and adapt them into your solution structure.
- Track the docs site and release notes for module-specific guidance as packages ship.

Success signals to look for:

- Logs and traces include correlation IDs and, when applicable, tenant identifiers.
- Validation errors are predictable and localized to the entry points where they belong.
- Pipeline ordering is explicit and documented in your solution.
- Teams can add or remove an extension via NuGet plus configuration without code churn.
- Samples and docs in this repo stay aligned with what you have in production.

### Compliance and audit readiness

Plan for Audit to capture who did what and when, using Core's time and correlation primitives. Pair with Authorization and Settings to enforce policies and runtime overrides. Localization will help surface user-facing messages in the right language, and FeatureFlags can gate risky changes. Samples will include audit trails for commands and data changes.

How to get started:

- Identify which extensions are already available (Core and Validation today) and wire them following the quickstart.
- Read the matching HighLevelDocs entries for upcoming modules to understand required seams in your code.
- Sketch pipeline ordering and configuration toggles so adoption is a config change, not a refactor.
- Run the samples closest to your scenario and adapt them into your solution structure.
- Track the docs site and release notes for module-specific guidance as packages ship.

Success signals to look for:

- Logs and traces include correlation IDs and, when applicable, tenant identifiers.
- Validation errors are predictable and localized to the entry points where they belong.
- Pipeline ordering is explicit and documented in your solution.
- Teams can add or remove an extension via NuGet plus configuration without code churn.
- Samples and docs in this repo stay aligned with what you have in production.

### Developer experience at scale

Templates and CLI will let teams scaffold new services with extensions pre-wired. Testing utilities will standardize handler tests with deterministic clocks and fake loggers. NuGetPackaging and SourceLinkAndSymbols keep packages consistent. This path is for organizations standardizing on Clean Architecture and needing reproducible setups across squads.

How to get started:

- Identify which extensions are already available (Core and Validation today) and wire them following the quickstart.
- Read the matching HighLevelDocs entries for upcoming modules to understand required seams in your code.
- Sketch pipeline ordering and configuration toggles so adoption is a config change, not a refactor.
- Run the samples closest to your scenario and adapt them into your solution structure.
- Track the docs site and release notes for module-specific guidance as packages ship.

Success signals to look for:

- Logs and traces include correlation IDs and, when applicable, tenant identifiers.
- Validation errors are predictable and localized to the entry points where they belong.
- Pipeline ordering is explicit and documented in your solution.
- Teams can add or remove an extension via NuGet plus configuration without code churn.
- Samples and docs in this repo stay aligned with what you have in production.

## Inspiration and gratitude

This project exists because of Jason Taylor's Clean Architecture template. The template gives teams a clear starting point; this repository gives them an ecosystem to grow without forking. If you are new here, start by reading Jason's repository: https://github.com/jasontaylordev/CleanArchitecture. Then come back and plug in only what you need.

## Next steps for readers

- If you want to build with what exists today: install Core and Validation, run the samples, read the Core and Validation docs, and wire the behaviors into your pipeline.
- If you want to contribute: pick a work-in-progress module, read its HighLevelDocs design, and start with docs and sample scaffolding before coding.
- If you want to keep tabs: watch the repo and the GitHub Pages site for release notes and roadmap updates.

---

This README is intentionally comprehensive and long-form to capture newcomers at first glance and give veterans the depth they need. The links above take you directly to the working docs, samples, and design plans. Use what you need today, and help shape what ships next.
