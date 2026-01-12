# Scenario: Header + Route Resolution with ASP.NET Core Enforcement

## Goal

Document a sample that shows deterministic tenant resolution from route first, host second, and header fallback, with ProblemDetails responses when a tenant is missing or inactive

## Sample name and location

- Solution: `CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution`
- Path: `samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution`

## Modules used

- Multitenancy core (resolution pipeline + behaviors)
- Multitenancy.AspNetCore (middleware, attributes, ProblemDetails)

## Prerequisites

- Install the .NET SDK required by the Clean Architecture template.
- Keep numbered step comments in code changes and mirror them in the sample README per repository guidance.

## Steps

1. Install Jason Taylor's Clean Architecture template and create the base Web API-only solution (no extensions yet).
   - Install or update the template to the version we align with:
     ```bash
     dotnet new install Clean.Architecture.Solution.Template::10.0.0-preview
     ```
   - From the repo root, create the sample solution under `CleanArchitecture.Extensions/samples` using SQLite:
     ```bash
     cd CleanArchitecture.Extensions/samples
     dotnet new ca-sln -cf None -o CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution --database sqlite
     ```
   - Verify the output folder exists and contains the new solution file plus `src/` and `tests/`.
2. Add NuGet package references for the multitenancy extensions (use the latest published versions from NuGet).
   - Packages: [CleanArchitecture.Extensions.Multitenancy](https://www.nuget.org/packages/CleanArchitecture.Extensions.Multitenancy) and [CleanArchitecture.Extensions.Multitenancy.AspNetCore](https://www.nuget.org/packages/CleanArchitecture.Extensions.Multitenancy.AspNetCore).
   - You can either use `dotnet add package` or edit the files directly as shown below. The sample uses central package management, so versions live in `Directory.Packages.props`.
   - `samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution/Directory.Packages.props`:
     ```xml
     <!-- Step 2: (Begin) Pin CleanArchitecture.Extensions package version -->
     <CleanArchitectureExtensionsVersion>0.1.1-preview.1</CleanArchitectureExtensionsVersion>
     <!-- Step 2: (End) Pin CleanArchitecture.Extensions package version -->
     ```
     ```xml
     <!-- Step 2: (Begin) Add Multitenancy package versions -->
     <PackageVersion Include="CleanArchitecture.Extensions.Multitenancy" Version="$(CleanArchitectureExtensionsVersion)" />
     <PackageVersion Include="CleanArchitecture.Extensions.Multitenancy.AspNetCore" Version="$(CleanArchitectureExtensionsVersion)" />
     <!-- Step 2: (End) Add Multitenancy package versions -->
     ```
   - `samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution/src/Application/Application.csproj`:
     ```xml
     <!-- Step 2: (Begin) Add Multitenancy core package -->
     <PackageReference Include="CleanArchitecture.Extensions.Multitenancy" />
     <!-- Step 2: (End) Add Multitenancy core package -->
     ```
   - `samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution/src/Web/Web.csproj`:
     ```xml
     <!-- Step 2: (Begin) Add Multitenancy AspNetCore package -->
     <PackageReference Include="CleanArchitecture.Extensions.Multitenancy.AspNetCore" />
     <!-- Step 2: (End) Add Multitenancy AspNetCore package -->
     ```
3. Configure `MultitenancyOptions` for route-first ordering (`Route > Host > Header > Query > Claim`), set header name `X-Tenant-ID`, require tenants by default, and disable fallback tenants.
4. Register services with `AddCleanArchitectureMultitenancy` then `AddCleanArchitectureMultitenancyAspNetCore(autoUseMiddleware: false)`; place `UseCleanArchitectureMultitenancy` after routing and before authentication/authorization.
5. Add route conventions that group tenant-bound APIs under `/tenants/{tenantId}/...`; keep health/status endpoints outside the group for anonymous access.
6. Decorate tenant-bound endpoints with `RequireTenant`, and mark public endpoints with `AllowAnonymousTenant` to keep resolution optional without enforcement.
7. Enable `TenantExceptionHandler`/ProblemDetails so unresolved tenants return 400, missing tenants return 404, and suspended tenants return 403.
8. Add integration tests that cover: resolved via route, resolved via host mapping, header fallback when the route is absent, conflict handling when route/header disagree, and enforcement responses when no tenant is provided.
9. Update the sample README with the walkthrough (inputs, expected status codes) and middleware ordering reminders.

## Validation

- Requests with `/tenants/{tenantId}` succeed only when the tenant exists and is active.
- Requests without tenant context return the expected ProblemDetails payloads.
- Tenant context is cleared per request (no AsyncLocal leakage between tests).
