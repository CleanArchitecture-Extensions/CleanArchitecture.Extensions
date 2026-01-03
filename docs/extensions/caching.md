# Extension: Caching

## Overview

CleanArchitecture.Extensions.Caching provides cache abstractions, deterministic key generation, and a MediatR query caching behavior. It is provider-agnostic and can target in-memory or distributed caches without leaking infrastructure concerns into handlers.

## When to use

- You want transparent query caching without embedding cache calls in handlers.
- You need deterministic, namespace-aware cache keys.
- You want to start with memory caching in development and switch to distributed cache in production.

## Prereqs and compatibility

- Target framework: `net10.0`.
- Dependencies: MediatR `13.1.0`, `Microsoft.Extensions.Caching.*` `10.0.0`.

## Install

```powershell
dotnet add src/Application/Application.csproj package CleanArchitecture.Extensions.Caching
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Caching
```

## Register services

```csharp
using CleanArchitecture.Extensions.Caching;
using CleanArchitecture.Extensions.Caching.Options;

builder.Services.AddCleanArchitectureCaching(options =>
{
    options.DefaultNamespace = "MyApp";
    options.MaxEntrySizeBytes = 256 * 1024;
}, behaviorOptions =>
{
    behaviorOptions.DefaultTtl = TimeSpan.FromMinutes(5);
});
```

## Add the MediatR behavior

```csharp
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddCleanArchitectureCachingPipeline();
});
```

## How query caching works

`QueryCachingBehavior<TRequest, TResponse>` applies cache-aside semantics:

- The default predicate caches requests that opt in via `ICacheableQuery` or `[CacheableQuery]`.
- The cache key uses the request type name as the resource and a SHA256 hash of the request payload.
- Cache hits short-circuit the handler; cache misses store the handler result.

Opt-in a query by marker interface or attribute:

```csharp
using CleanArchitecture.Extensions.Caching;
using CleanArchitecture.Extensions.Caching.Abstractions;

[CacheableQuery]
public record GetTodosQuery : IRequest<TodosVm>;

// or
public record GetUserQuery(int Id) : IRequest<UserDto>, ICacheableQuery;
```

Configure request selection and TTLs via `QueryCachingBehaviorOptions`:

```csharp
builder.Services.AddCleanArchitectureCaching(
    configureQueryCaching: options =>
    {
        options.CachePredicate = request => request is ICacheableQuery;
        options.DefaultTtl = TimeSpan.FromMinutes(2);
        options.TtlByRequestType[typeof(GetUserQuery)] = TimeSpan.FromSeconds(30);
        options.CacheNullValues = false;
    });
```

## Cache keys and scopes

- Key format: `{namespace}:{tenant?}:{resource}:{hash}`.
- `DefaultCacheKeyFactory` hashes the request payload as JSON (deterministic SHA256).
- `ICacheScope` supplies the namespace and optional tenant segment.

If you customize keys, keep them deterministic and stable across versions. For user-scoped data, include user context in the hash or namespace.

## Choose a cache adapter

The default `ICache` implementation is `MemoryCacheAdapter`.

!!! note
    The memory adapter is process-local. In a multi-instance deployment, use a distributed cache.

To use a distributed cache, register `IDistributedCache` and swap the adapter:

```csharp
using CleanArchitecture.Extensions.Caching.Adapters;
using Microsoft.Extensions.Caching.StackExchangeRedis;

builder.Services.AddCleanArchitectureCaching();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "<redis-connection-string>";
});

builder.Services.AddSingleton<ICache, DistributedCacheAdapter>();
```

## Serialization

The default serializer is `SystemTextJsonCacheSerializer`. Replace it when needed:

```csharp
using CleanArchitecture.Extensions.Caching.Serialization;

builder.Services.AddSingleton<ICacheSerializer>(sp =>
    new SystemTextJsonCacheSerializer(new JsonSerializerOptions(JsonSerializerDefaults.Web)));
```

When multiple serializers are registered, set `CachingOptions.PreferredSerializer` to a content type or serializer type name.

## Stampede protection and entry options

- `CachingOptions.StampedePolicy` controls locking, timeouts, and jitter.
- `CachingOptions.DefaultEntryOptions` defines expiration, priority, and size hints.

```csharp
builder.Services.AddCleanArchitectureCaching(options =>
{
    options.StampedePolicy = new CacheStampedePolicy
    {
        EnableLocking = true,
        LockTimeout = TimeSpan.FromSeconds(3),
        Jitter = TimeSpan.FromMilliseconds(50)
    };
});
```

## Invalidation guidance

Caching is read-through; invalidation is explicit. On command success or domain events, remove keys:

```csharp
await cache.RemoveAsync(cacheScope.Create("GetUserQuery", hash));
```

Keep key conventions stable and consider bumping the namespace for breaking DTO changes.

## Multitenancy integration

If you use multitenancy, install the adapter and call `AddCleanArchitectureMultitenancyCaching` to include tenant IDs in cache keys:

```powershell
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Multitenancy.Caching
```

```csharp
builder.Services.AddCleanArchitectureCaching();
builder.Services.AddCleanArchitectureMultitenancyCaching();
```

## Observability

- `QueryCachingBehavior` logs cache hits and misses at `Debug` level.
- Adapters log warnings on oversized payloads or deserialization failures.

## Troubleshooting

- Cache is never hit: ensure the request type matches the cache predicate and the behavior is registered.
- Missing tenant in keys: install `CleanArchitecture.Extensions.Multitenancy.Caching` and call `AddCleanArchitectureMultitenancyCaching` after caching registration.
- Large payloads: raise `MaxEntrySizeBytes` or skip caching via `ResponseCachePredicate`.

## Samples and tests

See the caching tests under `tests/` for behavior coverage and usage patterns.

## Reference

- [Caching options](../reference/caching-options.md)
