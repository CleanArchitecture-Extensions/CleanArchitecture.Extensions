# Roadmap

Early-stage view of where CleanArchitecture.Extensions is heading. Status tags keep us honest about what is shipping now versus what is queued or still being explored.

## Status legend
- **Now**: active development and doc polish.
- **Next**: planned immediately after the current focus.
- **Exploring**: shaping scope and design; timeline not locked.
- **Later**: anchored idea, likely to follow once earlier phases land.

## Current focus
- Harden the Core extension (guards, `Result`/`Error`, correlation/logging/performance behaviors) and add quickstart-style samples.
- Finish the Multitenancy Core outline: tenant resolution pipeline, tenant context propagation, and doc coverage that matches the Clean Architecture template.

## Domains and extensions

### Foundations
- **Core** — Guard clauses, rich `Result`/`Error` primitives, logging/time abstractions, and pipeline behaviors for correlation, logging, and performance. _Status: Now._ Focus: API polish, samples that mirror Jason Taylor's template, and logging adapter guidance.

### Multitenancy
- **Multitenancy Core** — Tenant resolution strategies (host/header/route/claims), tenant context, and middleware/pipeline hooks to enforce tenant scope. _Status: Next._ Focus: finalize the resolution pipeline, document data-access patterns, and validate with a starter sample.
- **Tenant-aware caching helpers** — Key conventions and pipeline behaviors layered on the caching extension to prevent cross-tenant leakage. _Status: Exploring._ Focus: design cache key builders that compose with tenant resolution.

### Identity & Access
- **Authentication bridge** — Opinionated integration with ASP.NET Core Identity/external IdPs that fits the Clean Architecture layers and Core pipeline behaviors. _Status: Exploring._ Focus: align with the Authentication recipe and ensure multi-tenant awareness.
- **Authorization policies kit** — Simple policy helpers/guards for role-, permission-, and tenant-based checks in the Application and Domain layers. _Status: Exploring._

### Data & Caching
- **Caching extension** — Abstractions and behaviors for cache-first queries, invalidation helpers, and tenant-aware key builders. _Status: Planning._ Focus: lock the API surface, pick in-memory/distributed adapters, and wire it into the Caching recipe.
- **Resilience helpers** — Standardized retry/circuit-breaker wrappers for external calls with Core logging/correlation baked in. _Status: Later._ Focus: decide whether to ship as behaviors, decorators, or both.

### Observability & Operations
- **Logging adapters** — Bind `IAppLogger`/`ILogContext` to Serilog/`ILogger` with correlation and performance scopes. _Status: Exploring._ Focus: document adapter patterns and provide a Serilog starter.
- **Metrics and tracing hooks** — Minimal instrumentation around pipeline behaviors and background jobs. _Status: Later._ Focus: choose baseline OpenTelemetry conventions and keep them opt-in.

### Developer Experience
- **Samples and templates** — End-to-end sample solutions that combine Core + Multitenancy + caching/auth recipes. _Status: Next._ Focus: publish a minimal sample first, then layer on multitenancy and caching variants.
- **Documentation hygiene** — Keep MkDocs navigation, quick links, and extension templates in sync with shipped packages. _Status: Now._ Focus: update extension pages as APIs settle and keep this roadmap current.
