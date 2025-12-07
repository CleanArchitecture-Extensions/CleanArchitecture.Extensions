# CleanArchitecture.Extensions — home

Built for developers who start from [Jason Taylor's Clean Architecture template](https://github.com/jasontaylordev/CleanArchitecture) and refuse to fork it. This site is the narrative version of the long-form README: it explains what the ecosystem is, what ships today, how to install it in minutes, and what is coming next. Everything here is Markdown-first, wired to MkDocs, and backed by runnable samples so you can copy/paste with confidence.

## What this page gives you

- Understand how CleanArchitecture.Extensions stays aligned with Jason Taylor's template while adding opt-in capabilities.
- See the current catalog (Core, Validation, Exceptions) plus the roadmap across multitenancy, enterprise, SaaS, infrastructure, and developer experience.
- Install the preview packages in under ten minutes with copy-ready commands and pipeline wiring.
- Navigate docs, samples, recipes, and tests without guessing where things live.
- Pick an adoption path that matches your project (greenfield, migration, observability-first, SaaS, compliance-ready).

## Why this ecosystem exists (template-first, fork-free)

- The upstream template is the starting point; we do **not** fork it. Extensions arrive as NuGet packages that plug into the MediatR pipeline, middleware, and abstractions you already know.
- Every package mirrors the template's structure and dependency discipline. If you are comfortable in the original solution, you will be comfortable here.
- Opt-in is non-negotiable: install only what you need, toggle via configuration, and remove without rewriting handlers.
- Docs, samples, and tests live beside the code so the guidance you read matches the code you run.
- Correlation IDs, deterministic time, guards, and validation are provided without leaking infrastructure into Application or Domain layers.

## TL;DR quickstart (10 minutes)

1) Start from Jason Taylor's template if you have not already:

```powershell
dotnet new install Clean.Architecture.Solution.Template
```

2) Add the Core extension to your Application project to get correlation, logging, performance, results, guards, domain events, and time abstractions:

```powershell
dotnet add package CleanArchitecture.Extensions.Core --version 0.1.1-preview.1
```

Wire pipeline behaviors (ordering matches the template):

```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CorrelationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
```

3) Add Validation if you rely on FluentValidation in the pipeline:

```powershell
dotnet add package CleanArchitecture.Extensions.Validation --version 0.1.1-preview.1
```

Register validators and the behavior:

```csharp
services.AddValidatorsFromAssemblyContaining<CreateOrderValidator>();
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
```

4) Run the samples that match what you enabled. Open `CleanArchitecture.Extensions.sln`, pick a sample in `samples/`, and run it:

```powershell
dotnet run --project samples/CleanArchitecture.Extensions.Core.Pipeline.Sample/CleanArchitecture.Extensions.Core.Pipeline.Sample.csproj
```

5) Read the matching docs pages for details and options:

- Core overview: [extensions/core.md](extensions/core.md)
- Result primitives: [extensions/core-result-primitives.md](extensions/core-result-primitives.md)
- Pipeline behaviors: [extensions/core-pipeline-behaviors.md](extensions/core-pipeline-behaviors.md)
- Validation: [extensions/validation.md](extensions/validation.md)

## Status at a glance

- **Implemented (preview)**: `CleanArchitecture.Extensions.Core`, `CleanArchitecture.Extensions.Validation`, `CleanArchitecture.Extensions.Exceptions`
- **In design/build-out**: Caching, Multitenancy family, Enterprise extensions, SaaS extensions, Infrastructure adapters, Developer Experience toolchain (CLI, templates, testing, packaging).
- **Target frameworks**: `net10.0`
- **Packaging discipline**: SourceLink, XML docs, snupkg symbols for debugger-friendly consumption.
- **Docs**: MkDocs + Material; everything lives under `docs/` and is published to GitHub Pages.
- **Samples**: runnable under `samples/`; each mirrors a doc scenario.
- **Solution layout**: open `CleanArchitecture.Extensions/CleanArchitecture.Extensions.sln` to see `src`, `tests`, `samples`, `build`.

## How to use this docs site

- Start here to understand the ecosystem and pick your first package.
- Jump to [Getting started](getting-started/quickstart.md) for a copy/paste path if you just want to install.
- Use [Concepts](concepts/architecture-fit.md) to see how we honor Clean Architecture boundaries and composition rules.
- Browse the [Extensions catalog](extensions/index.md) for per-package deep dives, compat, install, usage, troubleshooting, and samples.
- Check [Recipes](recipes/authentication.md) for task-based guidance and [Samples](samples/index.md) for runnable walkthroughs.
- Track [Roadmap](roadmap.md) and [Release notes](release-notes/index.md) to see what is shipping next.
- Contributors: read [Contributing](contributing/index.md) and the HighLevelDocs design notes before sending a PR.

## Core principles (compatibility and discipline)

- **Template alignment**: pipeline order follows Jason's template: Correlation → Logging pre-processor → UnhandledException → Authorization → Validation → Performance → Handler.
- **Opt-in behaviors**: everything is additive. You can enable or disable behaviors via DI registration and options without rewriting handlers.
- **Deterministic tests**: `IClock` and `FrozenClock` keep time stable; results carry trace IDs; behaviors log correlation scopes.
- **Minimal dependencies**: stay close to `Microsoft.Extensions.*`, MediatR, and FluentValidation. Infrastructure adapters are optional packages.
- **Migration-friendly**: Legacy result shims let you migrate from the template's `Result` shape gradually.
- **Docs-first**: every capability ships with a doc page, sample, and tests that match. This page is the canonical overview; each subpage is the deep dive.

## Getting started (expanded walkthrough)

The quickstart above gets you running; this section adds context and verification steps.

### 1) Prepare a solution based on Jason Taylor's template

- Install the template and create a new solution, or open your existing template-based repo.
- Keep the upstream template untouched; you will add packages and behaviors only.

### 2) Add Core

Install and wire the behaviors:

```powershell
dotnet add package CleanArchitecture.Extensions.Core --version 0.1.1-preview.1
```

```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CorrelationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
```

Optional toggles (see [extensions/core-options.md](extensions/core-options.md)):

```csharp
services.Configure<CoreExtensionsOptions>(options =>
{
    options.CorrelationIdHeader = "X-Correlation-ID";
    options.PerformanceThresholdMilliseconds = 250;
    options.DefaultGuardStrategy = GuardStrategy.ReturnResult;
});
```

### 3) Add Validation

Install and register validators:

```powershell
dotnet add package CleanArchitecture.Extensions.Validation --version 0.1.1-preview.1
```

```csharp
services.AddValidatorsFromAssemblyContaining<CreateOrderValidator>();
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
```

Configure strategy (see [extensions/validation.md](extensions/validation.md)):

```csharp
services.Configure<ValidationOptions>(options =>
{
    options.Strategy = ValidationStrategy.ReturnResult;
    options.IncludePropertyName = true;
    options.LogValidationFailures = true;
    options.SeverityLogLevels[Severity.Error] = LogLevel.Warning;
});
```

### 4) Run and verify

- Run the pipeline sample to see correlation, logging, performance, and validation in action.
- Inspect logs for correlation IDs and performance warnings.
- Switch `ValidationOptions.Strategy` between Throw and ReturnResult to see behavior differences.

### 5) Adopt gradually

- Use Legacy result shims to migrate handlers without breaking controllers.
- Toggle behaviors on or off per environment via configuration options.
- Add more packages later as they ship; everything is composable.

## Shipped packages (deep highlights)

### CleanArchitecture.Extensions.Core

Focus: result primitives, guard clauses, correlation-aware logging, performance monitoring, domain events, time abstractions, options toggles.

- **Result and Result<T>**: carry status, error codes, messages, trace IDs, and combinators (`Map`, `Bind`, `Ensure`, `Combine`, `Recover`, `Tap`). Legacy shims (`LegacyResult`, `LegacyResult<T>`) help migrate from the template shape.
- **Guard clauses**: consistent validation for null/empty, ranges, enums, lengths, and state invariants. Configurable to throw or return `Result`.
- **Pipeline behaviors**: correlation, logging, and performance behaviors aligned to template ordering. Correlation assigns/propagates IDs; logging emits structured start/finish; performance warns when thresholds are exceeded.
- **Logging abstractions**: `IAppLogger` and `ILogContext` wrap your logging stack with scopes that include correlation metadata.
- **Domain events**: helpers for raising/dispatching events from aggregates while keeping Application clean.
- **Time abstractions**: `IClock`, `FrozenClock`, `OffsetClock` keep time deterministic across environments.
- **Options**: `CoreExtensionsOptions` centralize defaults for correlation IDs, thresholds, guard strategies, trace identifiers.
- **Docs**: start at [extensions/core.md](extensions/core.md) and follow links to result, guards, pipeline, logging, domain events, time, and options pages.
- **Samples**: see `samples/CleanArchitecture.Extensions.Core.*` for pipeline, logging, result, guards, domain events, time, and options.
- **Tests**: `tests/CleanArchitecture.Extensions.Core.Tests/` with result and legacy shims plus growing coverage for other pieces.

### CleanArchitecture.Extensions.Validation

Focus: predictable FluentValidation integration in the MediatR pipeline with correlation-aware logging and configurable strategies.

- **Behavior**: `ValidationBehaviour<TRequest, TResponse>` runs validators discovered via DI.
- **Strategies**: `Throw`, `ReturnResult`, or `Notify` (via `IValidationNotificationPublisher`).
- **Rule catalog**: `CommonRules` for trimmed strings, email, phone, pagination, culture codes, sort expressions, and optional values.
- **Logging**: integrates with `IAppLogger` and `ILogContext` to log failures with correlation IDs; map severities to log levels.
- **Docs**: [extensions/validation.md](extensions/validation.md) with install, options, strategies, troubleshooting.
- **Samples**: validation behavior is showcased in pipeline samples; more samples are planned as the catalog grows.
- **Planned**: tenant-aware rules, Minimal API/MVC filters, metrics hooks once Observability lands.

## Ecosystem and roadmap (preview)

This repo is a monorepo for a plug-in ecosystem. Only Core and Validation ship today; everything else is designed in `HighLevelDocs/` and staged under `docs/extensions/` as placeholders.

### Domain 1 — Core Architecture
- CleanArchitecture.Extensions.Core (shipped, preview)
- CleanArchitecture.Extensions.Validation (shipped, preview)
- CleanArchitecture.Extensions.Exceptions (shipped, preview)
- CleanArchitecture.Extensions.Caching (planned)

### Domain 2 — Multitenancy ecosystem
- Multitenancy core plus EFCore, AspNetCore, Identity, Sharding, Provisioning, Storage, Redis adapters (all planned; see [extensions/multitenancy-core.md](extensions/multitenancy-core.md) for intent).

### Domain 3 — Enterprise extensions
- Audit, Settings, FeatureFlags, Notifications, RateLimiting, Localization, Authorization (planned).

### Domain 4 — SaaS business extensions
- Payments, Documents, UserManagement (planned).

### Domain 5 — Infrastructure adapters
- Redis, MessageBus, Observability, Storage, Search (planned).

### Domain 6 — Developer Experience
- CLI, Templates, Testing, SourceLinkAndSymbols, NuGetPackaging (planned).

Roadmap depth and rationale live in `HighLevelDocs/CLEANARCHITECTURE.EXTENSIONS-MASTER-ROADMAP.md` and per-domain design docs. Each upcoming module will arrive with docs, samples, and compatibility guidance before code ships.

## Adoption playbooks (pick the one that matches you)

### Telemetry-first adoption

- Start with Core correlation, logging, and performance behaviors to light up tracing.
- Layer Validation with `ReturnResult` strategy to collect validation feedback without exceptions.
- Add Observability adapters later for OTEL export without touching handlers.
- Success signals: logs/traces include correlation IDs (and tenant IDs when multitenancy arrives); pipeline ordering is explicit; teams can toggle behaviors via config.

### Migration from the template Result

- Use Core's LegacyResult shims to bridge the template's `bool + errors` shape to richer results.
- Keep controllers intact while migrating handlers incrementally.
- Pair with Validation `ReturnResult` to avoid throwing where explicit outcomes are preferred.
- Success signals: handlers return richer metadata; no controller refactors; tests cover both shapes during transition.

### SaaS with tenant isolation (forward-looking)

- Plan for Multitenancy core with EFCore and AspNetCore adapters once released.
- Pair with RateLimiting and Caching using tenant-aware strategies.
- Connect Provisioning and Payments when billing arrives.
- Success signals: tenant boundaries enforced in data, HTTP, and caches; correlation and logs carry tenant identifiers.

### Event-driven integration

- Use Core domain events today; plan for MessageBus and Outbox patterns later.
- Keep correlation IDs flowing across process boundaries; use Observability for traces.
- Success signals: integration events carry trace/tenant context; handlers stay clean; retries and idempotency documented when adapters land.

### Compliance and audit readiness

- Plan for Audit (events, diffs, stores) plus Authorization and Settings for policy controls.
- Use Core time and correlation primitives for audit trails now.
- Success signals: who/what/when trails exist; configuration toggles gate risky behaviors; localization and feature flags help rollout safely.

### Developer experience at scale

- Watch for CLI, Templates, Testing, NuGetPackaging, and SourceLinkAndSymbols.
- Goal: reproducible setups, deterministic handler tests, consistent packaging, and debugger-friendly NuGets.
- Success signals: scaffolds reduce setup drift; packages include symbols and SourceLink; tests are deterministic via `IClock` and fakes.

## Samples and runnable stories

All samples live under `samples/` and are runnable via `dotnet run`. They mirror the docs so you can see real code wired into the pipeline.

- Pipeline: `samples/CleanArchitecture.Extensions.Core.Pipeline.Sample` — correlation, logging, performance behaviors.
- Logging: `samples/CleanArchitecture.Extensions.Core.Logging.Sample` — `IAppLogger`, scopes, correlation metadata.
- Result: `samples/CleanArchitecture.Extensions.Core.Result.Sample` — `Result`/`Result<T>`, legacy mapping, error aggregation.
- Guards: `samples/CleanArchitecture.Extensions.Core.Guards.Sample` — guard catalog and strategies.
- Domain events: `samples/CleanArchitecture.Extensions.Core.DomainEvents.Sample` — raising and dispatching events.
- Time: `samples/CleanArchitecture.Extensions.Core.Time.Sample` — `IClock`, `FrozenClock`, `OffsetClock`.
- Options: `samples/CleanArchitecture.Extensions.Core.Options.Sample` — toggling defaults for correlation IDs and thresholds.

Run a sample:

```powershell
dotnet run --project samples/CleanArchitecture.Extensions.Core.Result.Sample/CleanArchitecture.Extensions.Core.Result.Sample.csproj
```

## Documentation map (where to go next)

- Getting started: [getting-started/quickstart.md](getting-started/quickstart.md) · [getting-started/installation.md](getting-started/installation.md)
- Concepts: [concepts/architecture-fit.md](concepts/architecture-fit.md) · [concepts/composition.md](concepts/composition.md)
- Catalog: [extensions/index.md](extensions/index.md) with links to Core, Validation, Multitenancy placeholder, and future stubs.
- Package blueprints: [roadmap/package-blueprints.md](roadmap/package-blueprints.md) for the long-form per-extension blueprint formerly in the README.
- Recipes: [recipes/authentication.md](recipes/authentication.md) · [recipes/caching.md](recipes/caching.md)
- Samples: [samples/index.md](samples/index.md)
- Reference & Ops: [reference/configuration.md](reference/configuration.md) · [troubleshooting/index.md](troubleshooting/index.md) · [release-notes/index.md](release-notes/index.md) · [roadmap.md](roadmap.md)
- Adoption playbooks: [getting-started/adoption-playbooks.md](getting-started/adoption-playbooks.md) for scenario-driven guidance moved from the README.
- Contributing: [contributing/index.md](contributing/index.md)

## Quality, packaging, and compatibility promises

- **SourceLink and symbols**: every package ships with SourceLink and snupkg symbols for debugger-friendly consumption.
- **Target frameworks**: `net10.0` for shipped packages; future modules will follow the same strategy unless noted.
- **MediatR pipeline discipline**: behaviors are ordered to avoid surprises and to align with the template.
- **Logging and correlation**: consistent scopes and trace identifiers across behaviors, validation, and future adapters.
- **Docs and samples parity**: docs reference code that exists; samples are runnable; release notes call out breaking changes.
- **Exit ramps**: because everything is opt-in, you can uninstall a package and remove DI registrations to revert to the template baseline.

## Contribution signals

- Read the HighLevelDocs entry for the domain you want to touch (start at `HighLevelDocs/CLEANARCHITECTURE.EXTENSIONS-MASTER-ROADMAP.md` and domain-specific files).
- For shipped packages, add docs and samples alongside code changes.
- Follow the docs style guide: concise, imperative, code-first, language-tagged fences, minimal admonitions.
- Keep Jason Taylor's template untouched; add behaviors, middleware, and adapters via packages.
- Tests and samples should illustrate the docs you write; keep snippet drift low by referencing sample code.

## FAQ (fast answers)

- **Do I have to fork Jason's template?** No. Install packages and register behaviors; the template stays pristine.
- **Can I migrate gradually?** Yes. Use Legacy result shims, enable behaviors incrementally, and configure strategies per environment.
- **How do I know pipeline ordering?** See Core pipeline docs; we mirror the template order and call it out explicitly.
- **What frameworks are supported?** `net10.0` for shipped packages; others will state compatibility on their pages.
- **Where are tests and samples?** `tests/` and `samples/` in the solution; each doc page links to the relevant ones.
- **How do I track progress?** Watch [roadmap.md](roadmap.md) and [release-notes/index.md](release-notes/index.md); design intent lives in `HighLevelDocs/`.

## Next steps for you

- If you are evaluating: read Concepts, run the Core pipeline sample, and skim the Validation page.
- If you are adopting: follow the quickstart, wire Core and Validation, and keep pipeline ordering explicit in your solution.
- If you are contributing: pick a work-in-progress module, read the matching HighLevelDocs design, and start with docs and sample scaffolding before coding.
- If you want to keep tabs: watch the repo and the GitHub Pages site for release and roadmap updates.

This homepage stays long on purpose: it gives newcomers the full picture and gives veterans direct links to what matters. Use what you need today, keep your template clean, and help shape what ships next.
