# Architecture fit

CleanArchitecture.Extensions packages are designed to fit the Jason Taylor Clean Architecture template without changing its structure. The goal is to keep boundaries intact while adding cross-cutting behavior through composition.

## Design principles

- **No template fork**: packages integrate through DI, middleware, and MediatR behaviors.
- **Layered boundaries**: Application depends on abstractions, Infrastructure provides adapters, and the host wires everything up.
- **Opt-in by default**: each extension is optional and can be removed without structural changes.
- **Convention-aligned**: naming, registration patterns, and pipeline order match the template.

## Placement guidance

| Layer | Typical responsibilities | Extension examples |
| --- | --- | --- |
| Application | Behaviors and abstractions | `QueryCachingBehavior`, `TenantEnforcementBehavior`, `ICurrentTenant` |
| Infrastructure | Adapters and enforcement | `MemoryCacheAdapter`, `TenantSaveChangesInterceptor` |
| Host (Web/API) | Middleware and endpoint filters | `TenantResolutionMiddleware`, `TenantEnforcementEndpointFilter` |

## Template integration points

- `src/Application/DependencyInjection.cs`: register MediatR behaviors.
- `src/Infrastructure/DependencyInjection.cs`: register adapter services (caching, EF Core, etc.).
- `src/Web/Program.cs`: add middleware and endpoint filters.

## Removal strategy

- Remove the NuGet package reference.
- Remove `AddCleanArchitecture...` registrations and pipeline behaviors.
- Delete any configuration sections and validation stores that were introduced.
