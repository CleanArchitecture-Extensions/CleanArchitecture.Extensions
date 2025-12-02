# Architecture Fit

How extensions align with Jason Taylor's Clean Architecture template.

- Keep the template untouched: extensions plug in via packages, configuration, middleware/behaviors—not by forking or editing the upstream template.
- Preserve boundaries: respect domain/application/infrastructure/UI separation and dependency direction.
- Prefer composition: use pipeline behaviors, decorators, filters, and adapters instead of modifying core layers.
- Match conventions: mirror naming, folder structure, and coding style from the reference `JasonTaylorCleanArchitecture` copy.
- Stay optional: every extension should be opt-in with clear defaults and minimal required configuration.
