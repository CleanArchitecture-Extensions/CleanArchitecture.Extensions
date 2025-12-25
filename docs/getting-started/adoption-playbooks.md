# Adoption playbooks

## Caching-first adoption

Use caching for read-heavy queries and expensive lookups.

How to get started:

- Identify queries with high read frequency or expensive IO.
- Add the caching behavior and set a response predicate for what to cache.
- Add invalidation on command success or relevant domain events.

Success signals:

- Cache hit ratio climbs without stale data incidents.
- Latency for cached queries drops measurably.
- Cache keys are consistent and easy to reason about.

## SaaS with tenant isolation (planned)

Plan for Multitenancy Core so tenant context is available to caches, storage, and APIs.

How to get started:

- Define tenant resolution rules (header, route, host, claims).
- Decide where tenant enforcement should occur (middleware, behaviors).
- Ensure cache keys include tenant identifiers.

## Event-driven integration (planned)

Keep caches consistent when integration events flow between services.

How to get started:

- Use events to trigger cache invalidation.
- Track keys or tags so invalidation stays targeted.
- Keep cross-service cache coupling minimal and explicit.
