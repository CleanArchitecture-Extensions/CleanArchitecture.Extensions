# CleanArchitecture.Extensions.Caching

Cache abstractions and MediatR-friendly behaviors for Clean Architecture apps (in progress).

- Provider-agnostic `ICache` and serializer/key abstractions planned for memory and distributed stores.
- Default MemoryCache adapter is registered by <code>AddCleanArchitectureCaching</code>; a distributed adapter is available for <code>IDistributedCache</code> (resolve <code>DistributedCacheAdapter</code> or override <code>ICache</code> registration).
- Query caching behavior will plug into the MediatR pipeline without leaking infrastructure into handlers.
- Tenant-aware key conventions, stampede protection, and safe serialization defaults will align with the design blueprint.
- Ships with SourceLink, XML docs, and snupkg symbols like the other extensions once complete.

> Status: design staged; implementation is being built incrementally. API surface may change before the first preview.

## Planned usage

```csharp
using CleanArchitecture.Extensions.Caching;
using CleanArchitecture.Extensions.Caching.Options;
using MediatR;

// Register caching services and options
services.AddCleanArchitectureCaching(options =>
{
    options.Enabled = true;
    options.DefaultNamespace = "MyApp";
});

// Wire the query caching behavior (ordering finalized in later steps)
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddCleanArchitectureCachingPipeline(); // place after request checks, before performance behavior
});

// Swap to distributed cache (e.g., Redis) by overriding ICache registration
services.AddStackExchangeRedisCache(redis => redis.Configuration = "<redis-connection>");
services.AddSingleton<ICache, DistributedCacheAdapter>();
```

### Notes
- Keys follow `{namespace}:{tenant?}:{resource}:{hash}`; override `ResourceNameSelector`/`HashFactory` to control the resource or hash inputs.
- `QueryCachingBehaviorOptions` lets you set TTL per request type, cache predicate, and an optional response predicate to skip caching.
- Default adapters: memory (`ICache`) and a distributed adapter (`DistributedCacheAdapter`) for any `IDistributedCache` implementation.

## Target frameworks

- net10.0
