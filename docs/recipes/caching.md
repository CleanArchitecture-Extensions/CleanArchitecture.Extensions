# Recipe: Caching

## Goal
Add caching with clear cache key conventions and opt-in behaviors.

## Prereqs
- Base Clean Architecture template running.
- Choose cache store (in-memory, distributed) — adapters TBD.

## Steps
1. Add the cache adapter package (TBD).
2. Register cache services and behaviors.
3. Apply caching to queries/handlers where appropriate; define cache durations and invalidation rules.

## Verify
- First call hits data source; subsequent call hits cache (check logs/metrics).

## Pitfalls
- Cache stampede: add locking or jitter where needed.
- Tenant-aware caching: ensure keys include tenant context when multitenancy is enabled.
