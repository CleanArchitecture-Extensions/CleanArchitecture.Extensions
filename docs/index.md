# CleanArchitecture.Extensions

CleanArchitecture.Extensions is a small set of opt-in NuGet packages that plug into Jason Taylor's Clean Architecture template without forking it.

## What ships today

- `CleanArchitecture.Extensions.Caching`
- `CleanArchitecture.Extensions.Multitenancy`
- `CleanArchitecture.Extensions.Multitenancy.AspNetCore`

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

No other template changes required.

## Where to go next

- Caching docs: [extensions/caching.md](extensions/caching.md)
- Multitenancy docs: [extensions/multitenancy-core.md](extensions/multitenancy-core.md)
- Multitenancy.AspNetCore docs: [extensions/multitenancy-aspnetcore.md](extensions/multitenancy-aspnetcore.md)
- Extensions catalog: [extensions/index.md](extensions/index.md)
- Roadmap: [roadmap.md](roadmap.md)
