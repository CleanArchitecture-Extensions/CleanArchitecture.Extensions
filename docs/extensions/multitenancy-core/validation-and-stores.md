# Multitenancy Core: Validation and stores

This page explains validation modes and how to plug in tenant metadata stores and caches.

## Validation modes

`MultitenancyOptions.ValidationMode` controls how tenant IDs are validated:

- `None` (default): no lookup; the resolver creates a minimal active `TenantInfo`.
- `Cache`: validate only via `ITenantInfoCache`.
- `Repository`: validate via `ITenantInfoStore` and optionally cache results.

If a required cache/store is not registered, the system logs a warning and leaves the tenant as unvalidated. `TenantEnforcementBehavior` then throws `TenantNotFoundException` when a tenant is required.

## Implementing a store

```csharp
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;

public sealed class InMemoryTenantStore : ITenantInfoStore
{
    private readonly Dictionary<string, ITenantInfo> _tenants =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["alpha"] = new TenantInfo("alpha") { Name = "Alpha", IsActive = true },
            ["beta"] = new TenantInfo("beta") { Name = "Beta", IsActive = true }
        };

    public Task<ITenantInfo?> FindByIdAsync(string tenantId, CancellationToken cancellationToken = default)
        => Task.FromResult(_tenants.TryGetValue(tenantId, out var tenant) ? tenant : null);
}
```

## Implementing a cache

```csharp
using CleanArchitecture.Extensions.Multitenancy.Abstractions;

public sealed class InMemoryTenantCache : ITenantInfoCache
{
    private readonly Dictionary<string, ITenantInfo> _cache = new(StringComparer.OrdinalIgnoreCase);

    public Task<ITenantInfo?> GetAsync(string tenantId, CancellationToken cancellationToken = default)
        => Task.FromResult(_cache.TryGetValue(tenantId, out var tenant) ? tenant : null);

    public Task SetAsync(ITenantInfo tenant, TimeSpan? ttl, CancellationToken cancellationToken = default)
    {
        _cache[tenant.TenantId] = tenant;
        return Task.CompletedTask;
    }
}
```

Register the store/cache in DI and enable validation:

```csharp
using CleanArchitecture.Extensions.Multitenancy.Configuration;

services.AddSingleton<ITenantInfoStore, InMemoryTenantStore>();
services.AddSingleton<ITenantInfoCache, InMemoryTenantCache>();

services.Configure<MultitenancyOptions>(options =>
{
    options.ValidationMode = TenantValidationMode.Repository;
    options.ResolutionCacheTtl = TimeSpan.FromMinutes(10);
});
```

## Fallback tenant

The default provider can return a fallback tenant if configured:

```csharp
services.Configure<MultitenancyOptions>(options =>
{
    options.FallbackTenantId = "local";
});
```

Fallbacks are only used when the `DefaultTenantProvider` runs (source `Default`).

## Lifecycle checks

`TenantEnforcementBehavior` treats a tenant as invalid when:

- `IsActive` is false
- `IsSoftDeleted` is true
- `State` is `Suspended`, `PendingProvision`, or `Deleted`
- `ExpiresAt` has passed

Populate these fields in your `ITenantInfoStore` so enforcement works as expected.
