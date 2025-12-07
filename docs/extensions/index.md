# Extensions Catalog

Pick the extensions you need without touching Jason Taylor's upstream template. Each page follows a common structure: overview, when to use, compatibility, install, usage, troubleshooting, samples/tests.

## Shipped (preview)
- **CleanArchitecture.Extensions.Core** — foundation primitives (guards, Result/Error, logging/correlation/performance behaviors, domain events, time, options): [Core](core.md) · [Options](core-options.md) · [Time](core-time.md) · [Domain events](core-domain-events.md) · [Logging abstractions](core-logging-abstractions.md) · [Pipeline behaviors](core-pipeline-behaviors.md) · [Guard clauses](core-guard-clauses.md) · [Result primitives](core-result-primitives.md)
- **CleanArchitecture.Extensions.Validation** — FluentValidation behavior + strategies, rule catalog, correlation-aware logging: [Validation](validation.md)

## In design / planned (placeholders until code lands)
- **Multitenancy Core** — tenant resolution/enforcement primitives: [Multitenancy Core](multitenancy-core.md)
- Additional modules (Exceptions, Caching, Multitenancy adapters, Enterprise, SaaS, Infrastructure, DX) are tracked in the [Roadmap](../roadmap.md) and `HighLevelDocs/*`.

## How to use this catalog
- Start with Core; add Validation when you use FluentValidation in the MediatR pipeline.
- For upcoming modules, read the placeholder page plus the matching HighLevelDocs file before contributing code.
- Keep compatibility info (template version, target frameworks, dependencies) current on each page.
- Link samples and tests: every shipped module should reference a runnable sample under `samples/` and relevant tests under `tests/`.
