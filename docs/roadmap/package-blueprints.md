# Package blueprints

## CleanArchitecture.Extensions.Caching (shipped)

What it provides:

- Cache abstractions with memory and distributed adapters.
- Query caching behavior for MediatR.
- Simple configuration for expiration and cache predicates.

Design notes:

- HighLevelDocs/Domain1-CoreArchitectureExtensions/CleanArchitecture.Extensions.Caching.md
- Docs: [docs/extensions/caching.md](../extensions/caching.md)

## CleanArchitecture.Extensions.Multitenancy (shipped)

What it provides:

- Tenant model and current tenant abstraction.
- Resolution providers (header, route, host, claims, default).
- Validation hooks (cache/store) and enforcement behaviors.
- Context serialization and cache scope integration.

Design notes:

- HighLevelDocs/Domain2-Multitenancy/CleanArchitecture.Extensions.Multitenancy.md
- Docs: [docs/extensions/multitenancy-core.md](../extensions/multitenancy-core.md)

## CleanArchitecture.Extensions.Multitenancy.AspNetCore (shipped)

What it provides:

- Middleware and endpoint helpers for tenant resolution.
- Minimal API and MVC enforcement filters.

Design notes:

- HighLevelDocs/Domain2-Multitenancy/CleanArchitecture.Extensions.Multitenancy.AspNetCore.md
- Docs: [docs/extensions/multitenancy-aspnetcore.md](../extensions/multitenancy-aspnetcore.md)

## CleanArchitecture.Extensions.Multitenancy.EFCore (planned)

What it will provide:

- Tenant-aware DbContext helpers and filters.
- Patterns for shared and schema-per-tenant databases.

Design notes:

- HighLevelDocs/Domain2-Multitenancy/CleanArchitecture.Extensions.Multitenancy.EFCore.md

## CleanArchitecture.Extensions.Multitenancy.Identity (planned)

What it will provide:

- Tenant-aware Identity helpers and policies.

Design notes:

- HighLevelDocs/Domain2-Multitenancy/CleanArchitecture.Extensions.Multitenancy.Identity.md
