# Extension: Multitenancy.EFCore

## Overview

CleanArchitecture.Extensions.Multitenancy.EFCore adds EF Core helpers for tenant isolation: query filters, SaveChanges enforcement, schema handling, and tenant-aware DbContext factories.

## When to use

- You want row-level isolation in a shared database.
- You need schema-per-tenant isolation with separate EF Core model caches.
- You want SaveChanges to enforce tenant ownership automatically.

## Prereqs and compatibility

- Target framework: `net10.0`.
- Dependencies: EF Core `10.0.0`.
- Requires multitenancy core and a tenant resolver in your host.

## Install

```powershell
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Multitenancy.EFCore
```

## Register services

```csharp
using CleanArchitecture.Extensions.Multitenancy.EFCore;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;

builder.Services.AddCleanArchitectureMultitenancyEfCore(options =>
{
    options.Mode = TenantIsolationMode.SharedDatabase;
    options.TenantIdPropertyName = "TenantId";
    options.UseShadowTenantId = true;
});
```

Call `options.UseTenantModelCacheKeyFactory(sp)` when configuring your DbContext to enable tenant-aware model cache keys. If you need a custom `IModelCacheKeyFactory`, call `ReplaceService` after this line.

Row-level filtering/enforcement defaults to shared database mode. For schema/database-per-tenant setups, set `UseShadowTenantId`, `EnableQueryFilters`, and `EnableSaveChangesEnforcement` to `true` if you want row-level defense-in-depth.

## Configure DbContext

```csharp
using CleanArchitecture.Extensions.Multitenancy.EFCore.Interceptors;

builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("JaysonTaylorCleanArchitectureBlankDb"));
    options.AddInterceptors(sp.GetRequiredService<TenantSaveChangesInterceptor>());
    options.UseTenantModelCacheKeyFactory(sp);
});
```

If your host already registers `ISaveChangesInterceptor` instances, you can omit the explicit interceptor registration.

## Choose an integration style

### Option A: derive from TenantDbContext (non-Identity DbContext)

```csharp
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;

public sealed class ApplicationDbContext : TenantDbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentTenant currentTenant,
        IOptions<EfCoreMultitenancyOptions> optionsAccessor,
        ITenantModelCustomizer modelCustomizer)
        : base(options, currentTenant, optionsAccessor, modelCustomizer)
    {
    }
}
```

### Option B: keep IdentityDbContext (template default)

```csharp
using System.Reflection;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, ITenantDbContext
{
    private readonly ICurrentTenant _currentTenant;
    private readonly EfCoreMultitenancyOptions _multitenancyOptions;
    private readonly ITenantModelCustomizer _tenantModelCustomizer;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentTenant currentTenant,
        IOptions<EfCoreMultitenancyOptions> optionsAccessor,
        ITenantModelCustomizer tenantModelCustomizer)
        : base(options)
    {
        _currentTenant = currentTenant;
        _multitenancyOptions = optionsAccessor.Value;
        _tenantModelCustomizer = tenantModelCustomizer;
    }

    public string? CurrentTenantId => _currentTenant.TenantId;

    public ITenantInfo? CurrentTenantInfo => _currentTenant.TenantInfo;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        _tenantModelCustomizer.Customize(builder, this, _multitenancyOptions);
    }
}
```

## Tenant entities and global entities

Tenant-scoped entity:

```csharp
using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;

public sealed class Customer : ITenantEntity
{
    public int Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
}
```

Exclude global entities from filtering:

```csharp
using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;

[GlobalEntity]
public sealed class FeatureFlag
{
    public int Id { get; set; }
}
```

You can also mark global entities via `IGlobalEntity` or with `EfCoreMultitenancyOptions.GlobalEntityTypes`.

### Identity entities

By default, ASP.NET Core Identity entities are treated as global to keep the template working without extra changes. Set `TreatIdentityEntitiesAsGlobal = false` if you want tenant-scoped identity and add a tenant identifier to your Identity entities or custom stores.

## Schema-per-tenant setup

```csharp
builder.Services.AddCleanArchitectureMultitenancyEfCore(options =>
{
    options.Mode = TenantIsolationMode.SchemaPerTenant;
    options.SchemaNameFormat = "tenant_{0}";
});
```

In schema-per-tenant mode, the model cache key includes the schema when `UseTenantModelCacheKeyFactory(sp)` is enabled.

## Database-per-tenant setup

```csharp
builder.Services.AddCleanArchitectureMultitenancyEfCore(options =>
{
    options.Mode = TenantIsolationMode.DatabasePerTenant;
    options.ConnectionStringFormat = "Server=.;Database=Tenant_{0};Trusted_Connection=True;TrustServerCertificate=True;";
});

builder.Services.AddDbContextFactory<ApplicationDbContext>((sp, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("JaysonTaylorCleanArchitectureBlankDb"));
});

builder.Services.AddTenantDbContextFactory<ApplicationDbContext>();
```

For request-scoped DbContext registration, resolve the tenant connection inside `AddDbContext`:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    var currentTenant = sp.GetRequiredService<ICurrentTenant>();
    var resolver = sp.GetRequiredService<ITenantConnectionResolver>();
    var connectionString = resolver.ResolveConnectionString(currentTenant.TenantInfo);

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Tenant connection string was not resolved.");
    }

    options.UseSqlServer(connectionString);
});
```

## Migrations per tenant

Use `TenantMigrationRunner<TContext>` to migrate tenants in sequence:

```csharp
await migrationRunner.RunAsync(tenants, cancellationToken);
```

## Key components

- `TenantDbContext`, `ITenantDbContext`, `TenantModelCustomizer`
- `TenantSaveChangesInterceptor`
- `TenantModelCacheKeyFactory`
- `ITenantDbContextFactory<TContext>` and `TenantMigrationRunner<TContext>`

## Related docs

- [Multitenancy core](multitenancy-core.md)
- [EF Core options](../reference/efcore-multitenancy-options.md)
- [Troubleshooting](../troubleshooting/multitenancy.md)
