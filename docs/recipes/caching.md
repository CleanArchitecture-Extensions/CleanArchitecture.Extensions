# Recipe: Caching

## Goal

Add query caching with clear key conventions and safe invalidation.

## Prereqs

- Base Clean Architecture template running.
- Decide on cache store (memory for development or `IDistributedCache` in production).

## Steps

1. Add the caching package.

```powershell
dotnet add src/Application/Application.csproj package CleanArchitecture.Extensions.Caching
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Caching
```

2. Register caching services and configure TTLs.

```csharp
builder.Services.AddCleanArchitectureCaching(
    options => { options.DefaultNamespace = "MyApp"; },
    behaviorOptions =>
    {
        behaviorOptions.DefaultTtl = TimeSpan.FromMinutes(5);
        behaviorOptions.CacheNullValues = false;
    });
```

3. Add the query caching pipeline behavior.

```csharp
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddCleanArchitectureCachingPipeline();
});
```

4. Add invalidation for write operations (commands or domain events).

```csharp
await cache.RemoveAsync(cacheScope.Create("GetOrdersQuery", hash));
```

## Verify

- First call hits the data source; the second call hits cache.
- Different request parameters produce different cache keys.

## Pitfalls

- Cache stampede: keep locking enabled and set jitter.
- Tenant-aware caching: call `AddCleanArchitectureMultitenancyCaching` when multitenancy is enabled.
- Caching error responses: use `ResponseCachePredicate` to skip them.
