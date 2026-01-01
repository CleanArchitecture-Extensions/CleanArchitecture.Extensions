# Quickstart

Choose the path that matches your first use case. Each path is designed to be copy-paste friendly with minimal changes to the template.

## Option 1: add caching (5 minutes)

### 1) Install packages

```powershell
dotnet add src/Application/Application.csproj package CleanArchitecture.Extensions.Caching
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Caching
```

### 2) Register caching services (Infrastructure)

```csharp
using CleanArchitecture.Extensions.Caching;

builder.Services.AddCleanArchitectureCaching();
```

### 3) Add the MediatR caching behavior (Application)

```csharp
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddCleanArchitectureCachingPipeline();
});
```

### 4) Verify

Run the same query twice and confirm the second request is served from cache (debug logs show cache hit/miss).

## Option 2: add multitenancy to an HTTP API (10 minutes)

### 1) Install packages

```powershell
dotnet add src/Application/Application.csproj package CleanArchitecture.Extensions.Multitenancy
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Multitenancy
dotnet add src/Web/Web.csproj package CleanArchitecture.Extensions.Multitenancy.AspNetCore
```

### 2) Register services and middleware

```csharp
using CleanArchitecture.Extensions.Multitenancy.AspNetCore;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Middleware;

builder.Services.AddCleanArchitectureMultitenancyAspNetCore();

var app = builder.Build();
app.UseCleanArchitectureMultitenancy();
```

### 3) Add the multitenancy pipeline behaviors

```csharp
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddCleanArchitectureMultitenancyPipeline();
});
```

### 4) Mark tenant-required endpoints

```csharp
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Routing;

app.MapGroup("/tenants/{tenantId}")
    .AddTenantEnforcement()
    .RequireTenant();
```

## Optional: add EF Core isolation

```powershell
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Multitenancy.EFCore
```

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

Next steps:
- [Installation guide](installation.md)
- [Caching extension](../extensions/caching.md)
- [Multitenancy core](../extensions/multitenancy-core.md)
- [Multitenancy.AspNetCore](../extensions/multitenancy-aspnetcore.md)
- [Multitenancy.EFCore](../extensions/multitenancy-efcore.md)
