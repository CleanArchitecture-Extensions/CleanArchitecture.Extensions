# Recipe: Caching

## Goal
Add caching with clear cache key conventions and opt-in behaviors.

## Prereqs
- Base Clean Architecture template running.
- Choose cache store (in-memory default or an `IDistributedCache` implementation for distributed caching).

## Steps
1. Add the caching package: `dotnet add package CleanArchitecture.Extensions.Caching`.
2. Register caching services and the query caching pipeline behavior (after request checks, before performance logging).
3. Configure cacheability predicate and TTLs per query type; choose memory (default) or distributed adapter.
4. Apply invalidation on command success/domain events where needed (`ICache.Remove`).

## Verify
- First call hits data source; subsequent call hits cache (check logs or set a breakpoint).
- Change input parameters and confirm a different cache key/hash is used.

## Pitfalls
- Cache stampede: add locking or jitter where needed.
- Tenant-aware caching: ensure keys include tenant context when multitenancy is enabled.
- Avoid caching error or transient responses; use `ResponseCachePredicate` to skip them.
