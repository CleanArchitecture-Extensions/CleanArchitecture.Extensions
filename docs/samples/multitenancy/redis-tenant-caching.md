# Scenario: Tenant-Scoped Redis Caching with Key Prefixing

## Goal
Document a sample that combines multitenancy caching behavior with Redis key prefixing to guarantee cache isolation per tenant.

## Sample name and location
- Solution: `CleanArchitecture.Extensions.Samples.Multitenancy.RedisCaching`
- Path: `samples/CleanArchitecture.Extensions.Samples.Multitenancy.RedisCaching`

## Modules used
- Multitenancy core
- Multitenancy.Caching
- Multitenancy.Redis (planned adapter)
- Multitenancy.AspNetCore for inbound resolution

## Prerequisites
- Base Web API solution; provide a Redis connection (local container or test instance) via configuration.
- Numbered step comments and matching README entries for every change.

## Steps
1. Reference `CleanArchitecture.Extensions.Multitenancy`, `CleanArchitecture.Extensions.Multitenancy.AspNetCore`, `CleanArchitecture.Extensions.Multitenancy.Caching`, and add `CleanArchitecture.Extensions.Multitenancy.Redis` when available; ensure distributed cache services are registered before the multitenancy adapters.
2. Configure `MultitenancyOptions` to require tenant context for cache-bearing operations and enable `TenantScopedCacheBehavior` in MediatR.
3. Configure `RedisKeyStrategyOptions` with a prefix pattern such as `{tenant}:{resource}:{hash}`, disable global keys, and set default TTLs per resource; wire `TenantRedisConnectionResolver` if different endpoints per tenant are needed.
4. Register Redis distributed cache (StackExchange.Redis) pointing at the dev instance; expose the connection string via configuration so tests can swap to in-memory fakes.
5. Update caching helpers to reject keys that lack tenant context; audit background jobs to ensure `TenantContext` is restored before cache usage.
6. Add integration tests to assert keys include tenant prefixing, cross-tenant cache pollution is prevented, and cache hits/misses are counted per tenant.
7. Document migration steps for changing prefix formats and how to flush a single tenant's keys safely during deletion without issuing a global flush.
8. Note operational guardrails: avoid flush-all commands, monitor keyspace size per tenant, and alert on prefix collisions.

## Validation
- Cache keys are consistently prefixed with the current tenant ID and optional environment/resource segments.
- Requests without tenant context fail fast when cache access is required.
- Cache isolation holds under concurrent cross-tenant load and during key format migrations.
