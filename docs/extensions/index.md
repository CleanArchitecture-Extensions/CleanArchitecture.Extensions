# Extensions Catalog

Pick the extensions you need without touching Jason Taylor's upstream template. Each page follows a common structure: overview, when to use, compatibility, install, usage, troubleshooting, samples/tests.

## Active focus
- **CleanArchitecture.Extensions.Caching** — cache abstractions, adapters, and optional MediatR query caching behavior: [Caching](caching.md)
- **Multitenancy Core** — tenant resolution/enforcement primitives (planned): [Multitenancy Core](multitenancy-core.md)

## Deprecated (legacy)
- **CleanArchitecture.Extensions.Core** — foundation primitives (guards, Result/Error, logging/correlation/performance behaviors, domain events, time, options): [Core](core.md) · [Options](core-options.md) · [Time](core-time.md) · [Domain events](core-domain-events.md) · [Logging abstractions](core-logging-abstractions.md) · [Pipeline behaviors](core-pipeline-behaviors.md) · [Guard clauses](core-guard-clauses.md) · [Result primitives](core-result-primitives.md)
- **CleanArchitecture.Extensions.Validation** — FluentValidation behavior + strategies, rule catalog, correlation-aware logging: [Validation](validation.md)
- **CleanArchitecture.Extensions.Exceptions** — exception catalog, base types, redaction, and MediatR wrapping to convert exceptions to Results: [Exceptions](exceptions.md)

## In design / planned (placeholders until code lands)
- Additional modules (Multitenancy adapters, Enterprise, SaaS, Infrastructure, DX) are tracked in the [Roadmap](../roadmap.md) and `HighLevelDocs/*`.

## How to use this catalog
- Prefer the template defaults; add Caching or Multitenancy only when you need them.
- For upcoming modules, read the placeholder page plus the matching HighLevelDocs file before contributing code.
- Keep compatibility info (template version, target frameworks, dependencies) current on each page.
- Link samples and tests: every shipped module should reference a runnable sample under `samples/` and relevant tests under `tests/`.
