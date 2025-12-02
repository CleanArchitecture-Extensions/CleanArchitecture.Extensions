# Composition & Invariants

Principles for combining extensions safely.

- Isolation: each extension should have clear dependencies and avoid implicit cross-talk.
- Pipelines first: prefer mediatr behaviors, filters, and decorators to hook in cross-cutting concerns.
- Config clarity: document required settings, defaults, and compat matrices; fail fast on invalid configs.
- Observability: emit structured logs/events for extension lifecycle (init, errors, important decisions).
- Compatibility: declare supported .NET versions and CleanArchitecture template versions per extension page.
- Exit strategy: provide guidance for disabling/removing an extension cleanly.
