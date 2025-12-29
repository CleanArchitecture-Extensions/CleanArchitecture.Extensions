# CleanArchitecture.Extensions

[![Docs](https://github.com/CleanArchitecture-Extensions/CleanArchitecture.Extensions/actions/workflows/docs.yml/badge.svg?branch=main)](https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions)
[![CI](https://github.com/CleanArchitecture-Extensions/CleanArchitecture.Extensions/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/CleanArchitecture-Extensions/CleanArchitecture.Extensions/actions/workflows/ci.yml)
[![Release](https://github.com/CleanArchitecture-Extensions/CleanArchitecture.Extensions/actions/workflows/release.yml/badge.svg?branch=main)](https://github.com/CleanArchitecture-Extensions/CleanArchitecture.Extensions/actions/workflows/release.yml)
[![CodeQL](https://github.com/CleanArchitecture-Extensions/CleanArchitecture.Extensions/actions/workflows/codeql.yml/badge.svg?branch=main)](https://github.com/CleanArchitecture-Extensions/CleanArchitecture.Extensions/actions/workflows/codeql.yml)
[![NuGet Packages](https://img.shields.io/badge/NuGet-Packages-004880?logo=nuget)](https://www.nuget.org/profiles/CleanArchitecture.Extensions)
[![.NET 10.0](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

CleanArchitecture.Extensions is a small set of opt-in NuGet packages that plug into Jason Taylor's Clean Architecture template without forking it. The goal is to stay aligned with the template's layering and conventions while adding focused capabilities.

Quick links:

- [Quickstart](https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/getting-started/quickstart/)
- [Extensions Catalog](https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/extensions/)
- [Roadmap](https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/roadmap/)
- [Docs index](https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/)

## Status

- Shipped: `CleanArchitecture.Extensions.Caching` (net10.0)
- Shipped: `CleanArchitecture.Extensions.Multitenancy` (core, host-agnostic, net10.0)
- Shipped: `CleanArchitecture.Extensions.Multitenancy.AspNetCore` (ASP.NET Core adapters, net10.0)
- Planned: Multitenancy adapters (EFCore, Identity, Provisioning, Redis, Sharding, Storage)

## Getting started

### Caching

1. Install:
   ```powershell
   dotnet add package CleanArchitecture.Extensions.Caching
   ```
2. Register:
   ```csharp
   services.AddCleanArchitectureCaching();
   services.AddMediatR(cfg => cfg.AddCleanArchitectureCachingPipeline());
   ```
3. Configure caching options as needed.
4. No other template changes required.

### Multitenancy core

1. Install:
   ```powershell
   dotnet add src/Application/Application.csproj package CleanArchitecture.Extensions.Multitenancy
   dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Multitenancy
   ```
2. Register:
   ```csharp
   services.AddCleanArchitectureMultitenancy();
   services.AddMediatR(cfg => cfg.AddCleanArchitectureMultitenancyPipeline());
   ```
3. Resolve tenants in your host and set the current context.

### Multitenancy.AspNetCore

1. Install:
   ```powershell
   dotnet add src/Web/Web.csproj package CleanArchitecture.Extensions.Multitenancy.AspNetCore
   ```
2. Register:
   ```csharp
   services.AddCleanArchitectureMultitenancyAspNetCore();
   ```
3. Add middleware:
   ```csharp
   app.UseCleanArchitectureMultitenancy();
   ```

Docs to read next:

- [Caching overview](https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/extensions/caching/)
- [Multitenancy core overview](https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/extensions/multitenancy-core/)
- [Multitenancy.AspNetCore overview](https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/extensions/multitenancy-aspnetcore/)
- [Extensions catalog](https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/extensions/)
- [Roadmap](https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/roadmap/)

## Repo layout

- `src/` packages
- `tests/` test projects
- `docs/` documentation site source
- `samples/` sample projects (empty today)

## Contributing

Start with the roadmap and the docs. Keep changes aligned with the Jason Taylor template conventions and layering.
