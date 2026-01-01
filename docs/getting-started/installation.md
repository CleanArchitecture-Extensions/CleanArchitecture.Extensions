# Installation

This guide shows how to reference the extensions from the default Jason Taylor Clean Architecture template layout.

## Choose packages

| Package | Typical projects | Purpose |
| --- | --- | --- |
| `CleanArchitecture.Extensions.Caching` | Application + Infrastructure | Cache abstractions, adapters, and query caching behavior. |
| `CleanArchitecture.Extensions.Multitenancy` | Application + Infrastructure | Core multitenancy abstractions, resolution, and pipeline behaviors. |
| `CleanArchitecture.Extensions.Multitenancy.AspNetCore` | Web/API | Middleware and endpoint enforcement for HTTP. |
| `CleanArchitecture.Extensions.Multitenancy.EFCore` | Infrastructure | EF Core filters, interceptors, and DbContext helpers. |

## Install packages

Caching:

```powershell
dotnet add src/Application/Application.csproj package CleanArchitecture.Extensions.Caching
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Caching
```

Multitenancy core:

```powershell
dotnet add src/Application/Application.csproj package CleanArchitecture.Extensions.Multitenancy
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Multitenancy
```

ASP.NET Core adapter:

```powershell
dotnet add src/Web/Web.csproj package CleanArchitecture.Extensions.Multitenancy.AspNetCore
```

EF Core adapter:

```powershell
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Multitenancy.EFCore
```

## Register services

Each package provides an `AddCleanArchitecture...` extension method. For example:

```csharp
builder.Services.AddCleanArchitectureCaching();
builder.Services.AddCleanArchitectureMultitenancy();
builder.Services.AddCleanArchitectureMultitenancyAspNetCore();
builder.Services.AddCleanArchitectureMultitenancyEfCore();
```

Add pipeline behaviors in Application:

```csharp
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddCleanArchitectureCachingPipeline();
    cfg.AddCleanArchitectureMultitenancyPipeline();
});
```

## Uninstall

```powershell
dotnet remove src/Application/Application.csproj package CleanArchitecture.Extensions.Caching
dotnet remove src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Caching
```

Adjust for the package(s) you installed.
