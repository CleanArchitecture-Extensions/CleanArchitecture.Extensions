# CleanArchitecture.Extensions.Multitenancy.EFCore

EF Core adapters for tenant-aware data isolation in the JaysonTaylorCleanArchitectureBlank template. This package adds model filters, SaveChanges enforcement, schema helpers, and tenant-aware DbContext primitives.

## Step 1 - Install the package

Install in the Infrastructure project:

```powershell
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Multitenancy.EFCore
```

## Step 2 - Register EF Core multitenancy services

File: `src/Infrastructure/DependencyInjection.cs`

```csharp
using CleanArchitecture.Extensions.Multitenancy.EFCore;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Interceptors;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;
using Microsoft.EntityFrameworkCore;

builder.Services.AddCleanArchitectureMultitenancyEfCore(options =>
{
    options.Mode = TenantIsolationMode.SharedDatabase;
    options.TenantIdPropertyName = "TenantId";
    options.UseShadowTenantId = true;
});

builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AddInterceptors(sp.GetRequiredService<TenantSaveChangesInterceptor>());
});
```

If you already register `ISaveChangesInterceptor` instances (as the template does), you can omit the explicit `TenantSaveChangesInterceptor` line and rely on `GetServices<ISaveChangesInterceptor>()`.

## Step 3 - Wire tenant context into your DbContext

### Option A: use the tenant-aware base class (non-Identity DbContext)

```csharp
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore;

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

The Jason Taylor template uses `IdentityDbContext`, so you can implement `ITenantDbContext` directly and apply the model customizer after your configurations.

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

Note: if you add your own `HasQueryFilter` calls, apply tenant filtering last by calling `ApplyTenantModel(builder)` (from `TenantDbContext`) or `_tenantModelCustomizer.Customize(...)` after your configurations.

## Step 4 - Mark tenant entities

Use explicit tenant identifiers or let the extension add a shadow property:

```csharp
using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;

public sealed class Customer : ITenantEntity
{
    public int Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
}
```

If you already use `BaseAuditableEntity`, you can either:
- Keep it as-is and rely on the shadow `TenantId` property.
- Implement `ITenantEntity` on your base entity to make the tenant column explicit.

To exclude global entities from filtering:

```csharp
using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;

[GlobalEntity]
public sealed class FeatureFlag
{
    public int Id { get; set; }
}
```

## Schema-per-tenant setup

```csharp
builder.Services.AddCleanArchitectureMultitenancyEfCore(options =>
{
    options.Mode = TenantIsolationMode.SchemaPerTenant;
    options.SchemaNameFormat = "tenant_{0}";
});
```

## What to expect

- Shared database mode automatically adds a tenant filter (`TenantId`) to all tenant-scoped entities.
- SaveChanges enforces tenant ownership and rejects cross-tenant updates.
- Schema-per-tenant mode applies the tenant schema and isolates EF Core model caching.
- Global entities are excluded when they implement `IGlobalEntity` or use `[GlobalEntity]`.
