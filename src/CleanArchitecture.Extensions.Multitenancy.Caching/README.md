# CleanArchitecture.Extensions.Multitenancy.Caching

Optional caching integration for Clean Architecture multitenancy. This package binds cache scopes to the current tenant and provides a MediatR behavior that warns on tenant/cache scope mismatches.

## What you get

- `TenantCacheScope` to include tenant IDs in cache keys.
- `TenantScopedCacheBehavior` to log warnings when the cache scope tenant does not match the current tenant.
- `AddCleanArchitectureMultitenancyCaching` to register the tenant-aware cache scope.

## Install

```powershell
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Caching
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Multitenancy
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Multitenancy.Caching
```

## Quickstart

```csharp
using CleanArchitecture.Extensions.Caching;
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Behaviors;

builder.Services.AddCleanArchitectureCaching();
builder.Services.AddCleanArchitectureMultitenancy();
builder.Services.AddCleanArchitectureMultitenancyCaching();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddOpenBehavior(typeof(TenantScopedCacheBehavior<,>));
});
```

## Notes

- Register caching before calling `AddCleanArchitectureMultitenancyCaching`.
- This package does not replace the caching behavior (`QueryCachingBehavior`); it only scopes keys to tenants.
