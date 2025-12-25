# Installation

## Install

```powershell
dotnet add package CleanArchitecture.Extensions.Caching
```

## Register

```csharp
services.AddCleanArchitectureCaching();
services.AddMediatR(cfg => cfg.AddCleanArchitectureCachingPipeline());
```

If you do not want automatic query caching, skip `AddCleanArchitectureCachingPipeline()` and call `ICache` directly.

## Uninstall

```powershell
dotnet remove package CleanArchitecture.Extensions.Caching
```
