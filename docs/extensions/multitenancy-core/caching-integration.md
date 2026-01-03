# Multitenancy core: caching integration

Use the caching integration to ensure cache keys include the current tenant ID.

## Setup

1) Install `CleanArchitecture.Extensions.Multitenancy.Caching`.
2) Register caching services.
3) Replace the cache scope with `TenantCacheScope`.
4) (Optional) add `TenantScopedCacheBehavior` to warn about mismatches.

```powershell
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Multitenancy.Caching
```

```csharp
using CleanArchitecture.Extensions.Caching;
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Behaviors;

builder.Services.AddCleanArchitectureCaching();
builder.Services.AddCleanArchitectureMultitenancyCaching();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddOpenBehavior(typeof(TenantScopedCacheBehavior<,>));
});
```

## What changes

- `TenantCacheScope` replaces `ICacheScope` and includes `TenantId` in generated keys.
- Cache key shape remains `{namespace}:{tenant}:{resource}:{hash}`.
- `TenantScopedCacheBehavior` logs a warning when the cache scope tenant does not match the current tenant.

## Troubleshooting

- If you see cache scope mismatch warnings, ensure tenant resolution runs before caching behaviors.
- `AddCleanArchitectureMultitenancyCaching` throws if caching services are not registered first.
