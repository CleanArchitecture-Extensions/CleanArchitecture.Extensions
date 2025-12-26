# Extension: Caching

## Overview
Cache abstractions, key conventions, and a MediatR query caching behavior for Clean Architecture applications. Ships memory and distributed adapters, deterministic key generation, and options for stampede protection and TTL tuning without leaking infrastructure into handlers.

## When to use

- You want query read-through caching without embedding cache calls in handlers.
- You need deterministic, namespace/tenant-aware cache keys and provider-agnostic entry options.
- You plan to start with in-memory caching for dev/test and swap to distributed stores (Redis via `IDistributedCache`) later.

## Prereqs & Compatibility

- Target frameworks: `net10.0`.
- Dependencies: MediatR `13.1.0`, `Microsoft.Extensions.Caching.Abstractions`, `Microsoft.Extensions.Caching.Memory` (defaults); distributed adapter uses `IDistributedCache` (MemoryDistributedCache by default).
- Pipeline fit: register `QueryCachingBehavior<,>` after authorization and request checks, and before performance logging to avoid skewing timing warnings.

## Install

```bash
dotnet add src/YourProject/YourProject.csproj package CleanArchitecture.Extensions.Caching
```

## Usage

### Register caching and pipeline behavior

```csharp
using CleanArchitecture.Extensions.Caching;
using CleanArchitecture.Extensions.Caching.Options;
using MediatR;

services.AddCleanArchitectureCaching(options =>
{
    options.DefaultNamespace = "MyApp";
    options.MaxEntrySizeBytes = 256 * 1024; // optional
}, queryOptions =>
{
    queryOptions.DefaultTtl = TimeSpan.FromMinutes(5);
    // Default predicate caches types whose names end with "Query"; override to use a marker instead:
    // queryOptions.CachePredicate = req => req is IQueryMarker;
});

services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddCleanArchitectureCachingPipeline(); // place after request checks
});
```

### Configure cache keys and TTLs

- Keys follow `{namespace}:{tenant?}:{resource}:{hash}` via `ICacheKeyFactory` and `ICacheScope`. Override `ResourceNameSelector`/`HashFactory` in `QueryCachingBehaviorOptions` for custom resource naming or hashing (e.g., when parameters should be normalized).
- Default TTL comes from `QueryCachingBehaviorOptions.DefaultTtl`; override per request type with `TtlByRequestType[typeof(MyQuery)] = TimeSpan.FromSeconds(30);`.
- `CachePredicate` controls which requests are cacheable. By default it caches request types whose names end with "Query"; override to use markers or explicit type checks. `ResponseCachePredicate` can skip caching for responses you do not want stored.

### Choose an adapter

- Memory (default): registered as `ICache` by `AddCleanArchitectureCaching`, uses `MemoryCacheAdapter` with stampede locking and jitter.
- Distributed: resolve `DistributedCacheAdapter` or replace `ICache` registration:

```csharp
services.AddCleanArchitectureCaching();
services.AddStackExchangeRedisCache(opts => opts.Configuration = "..."); // or other IDistributedCache
services.AddSingleton<ICache, DistributedCacheAdapter>(); // override default
```

### Entry options and stampede settings

- `CachingOptions.DefaultEntryOptions` sets absolute/sliding expiration, priority, and size hints.
- `CachingOptions.StampedePolicy` controls locking timeout and jitter for both adapters.
- `CacheEntryOptions` can be passed per call or mapped by request type inside the behavior.

### Response-aware caching

Use `QueryCachingBehaviorOptions.ResponseCachePredicate` to skip caching responses you want to exclude (for example, error payloads or partial results).

## Key components

- `ICache`, `CacheEntryOptions`, `CacheStampedePolicy`, `CacheKey`, `ICacheKeyFactory`, `ICacheScope`, `ICacheSerializer`.
- `MemoryCacheAdapter`, `DistributedCacheAdapter` (for `IDistributedCache`).
- `QueryCachingBehavior<TRequest,TResponse>` with configurable TTLs, hash selection, predicate, and response filtering.

## Pipeline ordering

- Recommended: Authorization → Request checks → **QueryCachingBehavior** → Performance/Logging → Handlers (align with the template order you already use).
- Place caching after request checks to avoid caching invalid requests and before performance logging to exclude cache hits from handler timing warnings.

## Invalidation guidance

- Cache-aside pattern: explicit `ICache.Remove` or `ICache.RemoveAsync` on command success or domain event handlers.
- Include versioning and tenant segments in keys to avoid collisions; adjust namespace when making breaking DTO changes.

## Backlog / Next Iteration

- Add PII/classification guardrails so sensitive payloads can be blocked or redirected to encrypted storage.
- Provide an optional encrypting serializer wrapper for distributed caches with guidance for key management.
- Expose instrumentation hooks (hits, misses, latency) without forcing a specific metrics provider.
- Document and/or implement schema-versioned key strategies to support DTO shape changes safely.

## Testing

- Use the default memory adapter for Application tests; distributed adapter can use `MemoryDistributedCache` for deterministic runs.
