# Package blueprints

This page summarizes the intent for each package and links to the detailed extension docs.

## CleanArchitecture.Extensions.Caching (shipped)

**Purpose**

- Cache abstractions with memory and distributed adapters.
- Query caching behavior for MediatR.
- Deterministic cache key conventions.

**Docs**

- [Caching extension](../extensions/caching.md)

## CleanArchitecture.Extensions.Multitenancy (shipped)

**Purpose**

- Tenant model and current tenant abstraction.
- Resolution providers (header, route, host, claims, default).
- Validation hooks and enforcement behaviors.

**Docs**

- [Multitenancy core](../extensions/multitenancy-core.md)

## CleanArchitecture.Extensions.Multitenancy.AspNetCore (shipped)

**Purpose**

- HTTP middleware for resolution.
- Minimal API and MVC enforcement filters.
- ProblemDetails mapping for tenant errors.

**Docs**

- [Multitenancy.AspNetCore](../extensions/multitenancy-aspnetcore.md)

## CleanArchitecture.Extensions.Multitenancy.EFCore (shipped)

**Purpose**

- Query filters and SaveChanges enforcement.
- Schema-per-tenant and database-per-tenant helpers.
- Tenant-aware DbContext factory and migration runner.

**Docs**

- [Multitenancy.EFCore](../extensions/multitenancy-efcore.md)

## Planned packages

- Multitenancy.Identity
- Multitenancy.Provisioning
- Multitenancy.Redis
- Multitenancy.Sharding
