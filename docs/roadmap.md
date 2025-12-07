# Roadmap

An honest, Jason-Taylor-aligned look at where CleanArchitecture.Extensions is headed. We ship extensions as opt-in NuGet packages that plug into the upstream template without forking it. This roadmap tracks what is ready today, what is being built next, and what is queued behind it. Use it alongside HighLevelDocs/\* for the detailed design intent of each module.

## Status legend

- **Shipped** – package available (currently preview), docs + samples live.
- **Now** – active implementation and doc polish.
- **Next** – starts immediately after current work; requirements captured in HighLevelDocs.
- **Exploring** – shaping scope/design; timeline flexible.
- **Later** – anchored idea, sequenced after earlier phases.

## Current release snapshot

- Shipped (preview): CleanArchitecture.Extensions.Core, CleanArchitecture.Extensions.Validation, CleanArchitecture.Extensions.Exceptions (net10.0, SourceLink, snupkg). Docs are live; Core/Validation samples are available; Exceptions sample is queued.
- Now: Core polish (API/compat tests), Validation polish (samples, rule catalog hardening), documentation sync.
- Next: Multitenancy Core design → first implementation spike + sample; roadmap-backed doc updates for Multitenancy and Caching.

## Near-term milestones (0–90 days)

- Core/Validation: finalize samples coverage, add missing recipes, keep nav/links aligned with README.
- Multitenancy Core: lock resolution/enforcement pipeline, DI patterns, and logging/correlation alignment; publish first sample.
- Caching (planning): finalize abstractions and behaviors; design tenant-safe keying conventions to pair with Multitenancy.
- Docs hygiene: replace placeholders in Getting Started, Reference, Recipes, Samples, Troubleshooting, and Release Notes with concrete guidance tied to shipped packages.

## Domains and extensions

### Domain 1 — Core Architecture Extensions

- **CleanArchitecture.Extensions.Core** — guards, rich Result/Error, logging/correlation/performance behaviors, domain events, time, options. _Status: Shipped (preview)_. Focus: API polish, compatibility tests, and adapter guidance.
- **CleanArchitecture.Extensions.Validation** — FluentValidation behavior, strategies (throw/result/notify), rule catalog, correlation-aware logging. _Status: Shipped (preview)_. Focus: samples + rule coverage.
- **CleanArchitecture.Extensions.Exceptions** — exception translation/wrapping, problem-details alignment, pipeline behavior. _Status: Shipped (preview)_. Focus: docs polish, HTTP adapter/sample wiring, and catalog hardening.
- **CleanArchitecture.Extensions.Caching** — cache abstractions, behaviors, invalidation hooks, tenant-safe keying. _Status: Planning_. Will align with Multitenancy and Redis adapters.

### Domain 2 — Multitenancy Ecosystem

- **Multitenancy Core** — tenant model, resolution providers (host/header/route/claims), middleware, enforcement/validation behaviors. _Status: Next_. First sample planned.
- **Multitenancy.EFCore** — shared DB/schema-per-tenant/DB-per-tenant filters, DbContext factories, migrations/seeding. _Status: Exploring_.
- **Multitenancy.AspNetCore** — minimal API/controllers helpers, endpoint filters, provider wiring. _Status: Exploring_.
- **Multitenancy.Identity** — tenant claim mapping, policy provider, role-per-tenant support. _Status: Exploring_.
- **Multitenancy.Sharding / Provisioning / Storage / Redis** — sharding strategies, onboarding workflows, tenant-aware storage/cache. _Status: Later/Exploring_ depending on adapter.

### Domain 3 — Enterprise Extensions

- **Audit**, **Settings**, **FeatureFlags**, **Notifications**, **RateLimiting**, **Localization**, **Authorization** — cross-cutting packages to add audit trails, runtime config, feature toggles, notification channels, throttling, localization, and permission models. _Status: Exploring/Later_. Each will align with Core correlation/time and Multitenancy when present.

### Domain 4 — SaaS Business Extensions

- **Payments**, **Documents**, **UserManagement** — SaaS-focused building blocks (billing, document generation, onboarding/MFA/passwordless). _Status: Exploring_. Will emit events for Notifications and Provisioning.

### Domain 5 — Infrastructure Adapters

- **Redis**, **MessageBus** (RabbitMQ/ASB/Kafka + outbox), **Observability** (OTEL), **Storage** (Blob/S3/local), **Search** (Elastic/Azure Search) — adapters to keep Application clean while swapping providers. _Status: Planning/Exploring_. Will reuse Core logging/correlation and Multitenancy context where relevant.

### Domain 6 — Developer Experience

- **CLI**, **Templates**, **Testing**, **SourceLinkAndSymbols**, **NuGetPackaging** — tooling to scaffold modules, keep packaging consistent, and improve test ergonomics. _Status: Exploring/Later_. Templates/CLI follow once Multitenancy/Caching patterns stabilize.

## Workstream priorities and dependencies

- **Docs-first**: every shipped capability must have a filled extension page, sample, and recipe. README links depend on this.
- **Sample-first**: new modules land with runnable samples; prioritize pipeline wiring (MediatR, DI) and logs showing correlation/tenant context.
- **Compatibility**: keep MediatR signatures and template ordering intact; legacy Result shims remain until migrations are trivial.
- **Tenant-aware design**: Caching, Redis, Storage, MessageBus, RateLimiting must consume Multitenancy abstractions to prevent cross-tenant leakage.
- **Observability alignment**: adapters should carry correlation IDs from Core; OTEL conventions to follow once Observability ships.

## How to engage

- Building with shipped packages: start with Core + Validation, run the samples under samples/, and follow the extension pages for wiring and options.
- Contributing: pick a WIP module, read the matching HighLevelDocs/Domain*/CleanArchitecture.Extensions.*.md, propose doc/sample scaffolds, then code + tests.
- Tracking progress: watch releases and docs/release-notes/ once populated; this roadmap will be updated as milestones move.
