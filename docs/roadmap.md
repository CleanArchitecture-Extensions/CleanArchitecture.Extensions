# Roadmap

This roadmap reflects the current focus and order of delivery. Items may shift as new requirements appear.

## Shipped (preview)

- **CleanArchitecture.Extensions.Caching**: cache abstractions, adapters, and query caching behavior.
- **Multitenancy Core**: tenant model, resolution providers, enforcement behavior.
- **Multitenancy.AspNetCore**: middleware and endpoint enforcement for HTTP.
- **Multitenancy.EFCore**: tenant-aware DbContext helpers, filters, and SaveChanges enforcement.

## Next up

- Multitenancy.Identity adapter (tenant-aware Identity stores).
- Multitenancy.Provisioning adapter (tenant onboarding workflows).
- Caching adapters for Redis and other distributed stores.

## Later

- Sharding, storage, and observability adapters.
- Expanded samples and recipe library.
- API/reference extraction and versioned docs.
