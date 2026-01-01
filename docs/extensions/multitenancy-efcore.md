# Extension: Multitenancy.EFCore

## Overview
Tenant-aware EF Core helpers that enforce isolation for shared databases, schema-per-tenant, and database-per-tenant configurations. Includes global query filters, SaveChanges enforcement, schema handling, and DbContext factory helpers.

## When to use
- You want row-level tenant isolation in a shared database.
- You need schema-per-tenant isolation with model cache separation.
- You want SaveChanges to enforce tenant ownership automatically.

## Prereqs & Compatibility
- Target frameworks: `net10.0`.
- Dependencies: EF Core `10.0.0`, Microsoft.Extensions.Options `10.0.0`.
- Requires multitenancy core and a tenant resolver in your host.

## Install

```bash
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Multitenancy.EFCore
```

## Quickstart

### Register services

```csharp
using CleanArchitecture.Extensions.Multitenancy.EFCore;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Interceptors;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;

builder.Services.AddCleanArchitectureMultitenancyEfCore(options =>
{
    options.Mode = TenantIsolationMode.SharedDatabase;
    options.TenantIdPropertyName = "TenantId";
    options.UseShadowTenantId = true;
});
```

### Configure DbContext

```csharp
builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AddInterceptors(sp.GetRequiredService<TenantSaveChangesInterceptor>());
});
```

If your host already registers `ISaveChangesInterceptor` instances, you can omit the explicit `TenantSaveChangesInterceptor` line and rely on `GetServices<ISaveChangesInterceptor>()`.

### Option A: Derive from TenantDbContext (non-Identity DbContext)

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

### Option B: Keep IdentityDbContext (template default)

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

### Tenant entities

```csharp
using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;

public sealed class Customer : ITenantEntity
{
    public int Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
}
```

If you already use `BaseAuditableEntity`, you can rely on the shadow `TenantId` property, or implement `ITenantEntity` on your base entity to make the tenant column explicit.

Exclude global entities:

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

## Database-per-tenant setup

```csharp
builder.Services.AddCleanArchitectureMultitenancyEfCore(options =>
{
    options.Mode = TenantIsolationMode.DatabasePerTenant;
    options.ConnectionStringFormat = "Server=.;Database=Tenant_{0};Trusted_Connection=True;TrustServerCertificate=True;";
});

builder.Services.AddDbContextFactory<ApplicationDbContext>((sp, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
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

`ITenantDbContextFactory<TContext>` sets the connection string per tenant for background tasks and migrations. Register your own resolver if connection strings live outside configuration.

## Key components

- `TenantDbContext` base class, `ITenantDbContext`, and `TenantModelCacheKeyFactory`.
- `TenantSaveChangesInterceptor` for tenant enforcement on writes.
- `TenantModelCustomizer` for filters and schema configuration.
- `TenantDbContextFactory<TContext>` + `TenantMigrationRunner<TContext>` for per-tenant connections and migrations.

## Related modules

- Multitenancy Core (shipped)
- Multitenancy.AspNetCore (shipped)
- Multitenancy.Identity (planned)
- Multitenancy.Provisioning (planned)
