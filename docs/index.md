# CleanArchitecture.Extensions

CleanArchitecture.Extensions is a small set of opt-in NuGet packages that plug into Jason Taylor's Clean Architecture template without forking it.

## What ships today

- `CleanArchitecture.Extensions.Caching`

## Quickstart

```powershell
dotnet add package CleanArchitecture.Extensions.Caching
```

```csharp
services.AddCleanArchitectureCaching();
services.AddMediatR(cfg => cfg.AddCleanArchitectureCachingPipeline());
```

No other template changes required.

## Where to go next

- Caching docs: [extensions/caching.md](extensions/caching.md)
- Extensions catalog: [extensions/index.md](extensions/index.md)
- Roadmap: [roadmap.md](roadmap.md)
