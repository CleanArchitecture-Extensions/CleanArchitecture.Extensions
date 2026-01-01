# CleanArchitecture.Extensions

CleanArchitecture.Extensions is a small set of opt-in NuGet packages that plug into Jason Taylor's Clean Architecture template without forking it.

## What ships today

- `CleanArchitecture.Extensions.Caching`
- `CleanArchitecture.Extensions.Multitenancy`
- `CleanArchitecture.Extensions.Multitenancy.AspNetCore`
- `CleanArchitecture.Extensions.Multitenancy.EFCore`

## Quickstart

Caching:

```powershell
dotnet add package CleanArchitecture.Extensions.Caching
```

```csharp
services.AddCleanArchitectureCaching();
services.AddMediatR(cfg => cfg.AddCleanArchitectureCachingPipeline());
```

Multitenancy:

```powershell
dotnet add package CleanArchitecture.Extensions.Multitenancy
```

```csharp
services.AddCleanArchitectureMultitenancy();
services.AddMediatR(cfg => cfg.AddCleanArchitectureMultitenancyPipeline());
```

Multitenancy.AspNetCore:

```powershell
dotnet add package CleanArchitecture.Extensions.Multitenancy.AspNetCore
```

```csharp
services.AddCleanArchitectureMultitenancyAspNetCore();
```

Multitenancy.EFCore:

```powershell
dotnet add package CleanArchitecture.Extensions.Multitenancy.EFCore
```

```csharp
services.AddCleanArchitectureMultitenancyEfCore();
```

No template fork required.

## Where to go next

- Caching docs: [extensions/caching.md](extensions/caching.md)
- Multitenancy docs: [extensions/multitenancy-core.md](extensions/multitenancy-core.md)
- Multitenancy.AspNetCore docs: [extensions/multitenancy-aspnetcore.md](extensions/multitenancy-aspnetcore.md)
- Multitenancy.EFCore docs: [extensions/multitenancy-efcore.md](extensions/multitenancy-efcore.md)
- Extensions catalog: [extensions/index.md](extensions/index.md)
- Roadmap: [roadmap.md](roadmap.md)
