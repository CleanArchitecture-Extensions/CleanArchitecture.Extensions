# Multitenancy Core: Caching integration

Use the caching integration to ensure cache keys include tenant context.

## Setup

1) Register caching services.
2) Replace the cache scope with `TenantCacheScope`.
3) (Optional) add `TenantScopedCacheBehavior` to warn about mismatches.

```csharp
using CleanArchitecture.Extensions.Caching;
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Behaviors;
using MediatR;

services.AddCleanArchitectureCaching();
services.AddCleanArchitectureMultitenancyCaching();

services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddOpenBehavior(typeof(TenantScopedCacheBehavior<,>));
});
```

## What changes

- `TenantCacheScope` replaces `ICacheScope` and includes `TenantId` in generated keys.
- Cache key shape remains `{namespace}:{tenant}:{resource}:{hash}` as defined by the caching module.

## Troubleshooting

- If you see "Cache scope tenant mismatch" warnings, ensure `ITenantAccessor` is set before caching behaviors run.
- `AddCleanArchitectureMultitenancyCaching` throws if caching services are not registered first.
