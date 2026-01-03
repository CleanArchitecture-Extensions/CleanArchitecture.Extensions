# CleanArchitecture.Extensions

CleanArchitecture.Extensions is a set of opt-in NuGet packages that plug into Jason Taylor's Clean Architecture template without forking it. Each package is designed to be composable, template-aligned, and easy to remove.

## What you get

- Caching abstractions and MediatR query caching behavior.
- Multitenancy core plus ASP.NET Core and EF Core adapters.
- Extension methods and options that keep integration minimal and explicit.

## How it fits the template

- Application layer: pipeline behaviors and abstractions (`ICache`, `ICurrentTenant`).
- Infrastructure layer: adapters and data-enforcement helpers (EF Core, cache providers).
- Host layer: middleware and endpoint filters for HTTP workloads.

## Packages (current)

- [CleanArchitecture.Extensions.Caching](extensions/caching.md)
- [CleanArchitecture.Extensions.Multitenancy (core)](extensions/multitenancy-core.md)
- [CleanArchitecture.Extensions.Multitenancy.AspNetCore](extensions/multitenancy-aspnetcore.md)
- [CleanArchitecture.Extensions.Multitenancy.EFCore](extensions/multitenancy-efcore.md)

## Quickstart: caching

```powershell
dotnet add src/Application/Application.csproj package CleanArchitecture.Extensions.Caching
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Caching
```

```csharp
using CleanArchitecture.Extensions.Caching;

builder.Services.AddCleanArchitectureCaching();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddCleanArchitectureCachingPipeline();
});
```

## Quickstart: multitenancy (core + ASP.NET Core)

```powershell
dotnet add src/Application/Application.csproj package CleanArchitecture.Extensions.Multitenancy
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Multitenancy
dotnet add src/Web/Web.csproj package CleanArchitecture.Extensions.Multitenancy.AspNetCore
```

```csharp
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore;

builder.Services.AddCleanArchitectureMultitenancyAspNetCore(autoUseMiddleware: true);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddCleanArchitectureMultitenancyCorrelationPreProcessor();
    cfg.AddCleanArchitectureMultitenancyPipeline();
});

var app = builder.Build();
```

Use manual middleware wiring when you need claim- or route-based resolution so you can place the middleware after authentication or routing.

If you use MediatR request logging pre-processors (template default), register `AddCleanArchitectureMultitenancyCorrelationPreProcessor` before logging so request logs include tenant context.
In the Jason Taylor template, keep the multitenancy pipeline after authorization behaviors so authorization runs first.
The correlation pre-processor registers a matching post-processor to clean up log scopes.

## Documentation map

- [Getting started](getting-started/quickstart.md)
- [Concepts](concepts/architecture-fit.md)
- [Extensions catalog](extensions/index.md)
- [Recipes](recipes/caching.md)
- [Reference](reference/configuration.md)
- [Troubleshooting](troubleshooting/index.md)
- [Roadmap](roadmap.md)

## Compatibility

- Target frameworks: `net10.0`.
- Designed to integrate with the Jason Taylor Clean Architecture template without modifying the template repository.
