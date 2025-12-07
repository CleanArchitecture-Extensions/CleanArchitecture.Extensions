# Package blueprints (from README Appendix A)

## Appendix A — package-by-package blueprint (long form)

## CleanArchitecture.Extensions.Core (shipped — Core Architecture) {#cleanarchitectureextensionscore}

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

## CleanArchitecture.Extensions.Validation (shipped — Core Architecture) {#cleanarchitectureextensionsvalidation}

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

## CleanArchitecture.Extensions.Exceptions (work in progress — Core Architecture) {#cleanarchitectureextensionsexceptions}

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

## CleanArchitecture.Extensions.Caching (work in progress — Core Architecture) {#cleanarchitectureextensionscaching}

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

## CleanArchitecture.Extensions.Multitenancy (work in progress — Multitenancy) {#cleanarchitectureextensionsmultitenancy}

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

## CleanArchitecture.Extensions.Multitenancy.EFCore (work in progress — Multitenancy) {#cleanarchitectureextensionsmultitenancyefcore}

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

## CleanArchitecture.Extensions.Multitenancy.AspNetCore (work in progress — Multitenancy) {#cleanarchitectureextensionsmultitenancyaspnetcore}

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

## CleanArchitecture.Extensions.Multitenancy.Identity (work in progress — Multitenancy) {#cleanarchitectureextensionsmultitenancyidentity}

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

## CleanArchitecture.Extensions.Multitenancy.Sharding (work in progress — Multitenancy) {#cleanarchitectureextensionsmultitenancysharding}

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

## CleanArchitecture.Extensions.Multitenancy.Provisioning (work in progress — Multitenancy) {#cleanarchitectureextensionsmultitenancyprovisioning}

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

## CleanArchitecture.Extensions.Multitenancy.Storage (work in progress — Multitenancy) {#cleanarchitectureextensionsmultitenancystorage}

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

## CleanArchitecture.Extensions.Multitenancy.Redis (work in progress — Multitenancy) {#cleanarchitectureextensionsmultitenancyredis}

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

## CleanArchitecture.Extensions.Audit (work in progress — Enterprise) {#cleanarchitectureextensionsaudit}

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

## CleanArchitecture.Extensions.Settings (work in progress — Enterprise) {#cleanarchitectureextensionssettings}

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

## CleanArchitecture.Extensions.FeatureFlags (work in progress — Enterprise) {#cleanarchitectureextensionsfeatureflags}

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

## CleanArchitecture.Extensions.Notifications (work in progress — Enterprise) {#cleanarchitectureextensionsnotifications}

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

## CleanArchitecture.Extensions.RateLimiting (work in progress — Enterprise) {#cleanarchitectureextensionsratelimiting}

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

## CleanArchitecture.Extensions.Localization (work in progress — Enterprise) {#cleanarchitectureextensionslocalization}

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

## CleanArchitecture.Extensions.Authorization (work in progress — Enterprise) {#cleanarchitectureextensionsauthorization}

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

## CleanArchitecture.Extensions.Payments (work in progress — SaaS) {#cleanarchitectureextensionspayments}

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

## CleanArchitecture.Extensions.Documents (work in progress — SaaS) {#cleanarchitectureextensionsdocuments}

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

## CleanArchitecture.Extensions.UserManagement (work in progress — SaaS) {#cleanarchitectureextensionsusermanagement}

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

## CleanArchitecture.Extensions.Redis (work in progress — Infrastructure) {#cleanarchitectureextensionsredis}

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

## CleanArchitecture.Extensions.MessageBus (work in progress — Infrastructure) {#cleanarchitectureextensionsmessagebus}

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

## CleanArchitecture.Extensions.Observability (work in progress — Infrastructure) {#cleanarchitectureextensionsobservability}

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

## CleanArchitecture.Extensions.Storage (work in progress — Infrastructure) {#cleanarchitectureextensionsstorage}

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

## CleanArchitecture.Extensions.Search (work in progress — Infrastructure) {#cleanarchitectureextensionssearch}

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

## CleanArchitecture.Extensions.CLI (work in progress — Developer Experience) {#cleanarchitectureextensionscli}

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

## CleanArchitecture.Extensions.Templates (work in progress — Developer Experience) {#cleanarchitectureextensionstemplates}

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

## CleanArchitecture.Extensions.Testing (work in progress — Developer Experience) {#cleanarchitectureextensionstesting}

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

## CleanArchitecture.Extensions.SourceLinkAndSymbols (work in progress — Developer Experience) {#cleanarchitectureextensionssourcelinkandsymbols}

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

## CleanArchitecture.Extensions.NuGetPackaging (work in progress — Developer Experience) {#cleanarchitectureextensionsnugetpackaging}

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
