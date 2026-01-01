# Adoption playbooks

These playbooks describe pragmatic, low-risk ways to adopt extensions in production without refactoring the template.

## Playbook: caching for read-heavy queries

**Goal**: reduce latency and database load for expensive reads while keeping handlers clean.

**Steps**

1. Identify queries with high read frequency or expensive IO.
2. Install caching and add the query caching behavior.
3. Configure TTLs and a cache predicate (start conservative).
4. Pick a cache adapter (memory first; distributed later).
5. Add explicit invalidation on command success or domain events.

**Success signals**

- Cache hit ratio increases without stale data incidents.
- P95 latency improves for the cached queries.
- Cache keys are predictable and easily traced in logs.

**Risks and mitigations**

- Stale data: use short TTLs and explicit invalidation.
- Cache stampede: enable locking and jitter (default).
- Oversized payloads: set `MaxEntrySizeBytes` and size hints.

## Playbook: SaaS multitenancy for HTTP APIs

**Goal**: resolve tenant context for every request, enforce it in handlers, and isolate data.

**Steps**

1. Decide resolution sources (route, header, host, or claim).
2. Add multitenancy core and the ASP.NET Core adapter.
3. Configure `MultitenancyOptions` (resolution order, headers, route name).
4. Add `AddCleanArchitectureMultitenancyPipeline` and mark endpoints as required.
5. Implement `ITenantInfoStore` and enable validation to prevent spoofing.
6. (Optional) Add EF Core isolation and enable SaveChanges enforcement.

**Success signals**

- Requests without tenant identifiers fail fast with clear errors.
- Tenant ID shows up in logs and traces.
- Data reads and writes are isolated by tenant.

**Risks and mitigations**

- Claim-based resolution requires authentication to run first; place middleware accordingly.
- Missing tenant store when validation is enabled will cause enforcement failures; register it early.
- In database-per-tenant mode, ensure connection strings are resolvable for all tenants.

## Playbook: event-driven invalidation (advanced)

**Goal**: keep caches in sync when data changes across services.

**Steps**

1. Define events for data mutations (commands and integration events).
2. Invalidate cache keys from event handlers (or a small adapter service).
3. Keep cache key conventions consistent to avoid global invalidation.

**Success signals**

- Cache invalidations are targeted and low-latency.
- Cache and database values stay aligned under load tests.
