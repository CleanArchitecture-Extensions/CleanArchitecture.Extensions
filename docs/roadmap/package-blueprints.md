# Package blueprints

## CleanArchitecture.Extensions.Caching (shipped)

What it provides:

- Cache abstractions with memory and distributed adapters.
- Query caching behavior for MediatR.
- Simple configuration for expiration and cache predicates.

Design notes:

- HighLevelDocs/Domain1-CoreArchitectureExtensions/CleanArchitecture.Extensions.Caching.md
- Docs: [docs/extensions/caching.md](../extensions/caching.md)

## CleanArchitecture.Extensions.Multitenancy (planned)

What it will provide:

- Tenant model and current tenant abstraction.
- Resolution providers (header, route, host, claims).
- Enforcement behaviors for tenant-aware pipelines.

Design notes:

- HighLevelDocs/Domain2-Multitenancy/CleanArchitecture.Extensions.Multitenancy.md
- Docs: [docs/extensions/multitenancy-core.md](../extensions/multitenancy-core.md)

## CleanArchitecture.Extensions.Multitenancy.EFCore (planned)

What it will provide:

- Tenant-aware DbContext helpers and filters.
- Patterns for shared and schema-per-tenant databases.

Design notes:

- HighLevelDocs/Domain2-Multitenancy/CleanArchitecture.Extensions.Multitenancy.EFCore.md

## CleanArchitecture.Extensions.Multitenancy.AspNetCore (planned)

What it will provide:

- Middleware and endpoint helpers for tenant resolution.
- Minimal API and controller integration points.

Design notes:

- HighLevelDocs/Domain2-Multitenancy/CleanArchitecture.Extensions.Multitenancy.AspNetCore.md

## CleanArchitecture.Extensions.Multitenancy.Identity (planned)

What it will provide:

- Tenant-aware Identity helpers and policies.

Design notes:

- HighLevelDocs/Domain2-Multitenancy/CleanArchitecture.Extensions.Multitenancy.Identity.md
