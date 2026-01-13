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
   - `samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution/src/Application/Application.csproj`:
     ```xml
     <!-- Step 2: (Begin) Add Multitenancy core package -->
     <PackageReference Include="CleanArchitecture.Extensions.Multitenancy" VersionOverride="0.2.7" />
     <!-- Step 2: (End) Add Multitenancy core package -->
     ```
   - `samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution/src/Web/Web.csproj`:
     ```xml
     <!-- Step 2: (Begin) Add Multitenancy AspNetCore package -->
     <PackageReference Include="CleanArchitecture.Extensions.Multitenancy.AspNetCore" VersionOverride="0.2.7" />
     <!-- Step 2: (End) Add Multitenancy AspNetCore package -->
     ```
3. Configure `MultitenancyOptions` for route-first ordering (`Route > Host > Header > Query > Claim`), set header name `X-Tenant-ID`, require tenants by default, allow explicitly anonymous endpoints, and disable fallback tenants.
   - `samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution/src/Web/DependencyInjection.cs`:
     ```csharp
     // Step 3: (Begin) Multitenancy configuration imports
     using CleanArchitecture.Extensions.Multitenancy;
     using CleanArchitecture.Extensions.Multitenancy.Configuration;
     // Step 3: (End) Multitenancy configuration imports
     ```
     ```csharp
     // Step 3: (Begin) Configure multitenancy resolution defaults
     builder.Services.Configure<MultitenancyOptions>(options =>
     {
     options.RequireTenantByDefault = true;
     options.AllowAnonymous = true;
         options.HeaderNames = new[] { "X-Tenant-ID" };
         options.ResolutionOrder = new List<TenantResolutionSource>
         {
             TenantResolutionSource.Route,
             TenantResolutionSource.Host,
             TenantResolutionSource.Header,
             TenantResolutionSource.QueryString,
             TenantResolutionSource.Claim
         };
         options.FallbackTenant = null;
         options.FallbackTenantId = null;
     });
     // Step 3: (End) Configure multitenancy resolution defaults
     ```
4. Register services with `AddCleanArchitectureMultitenancy` then `AddCleanArchitectureMultitenancyAspNetCore(autoUseMiddleware: false)`; add `UseCleanArchitectureMultitenancy` after routing and before authentication/authorization.
   - `samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution/src/Web/DependencyInjection.cs`:
     ```csharp
     // Step 4: (Begin) Multitenancy ASP.NET Core registration imports
     using CleanArchitecture.Extensions.Multitenancy.AspNetCore;
     // Step 4: (End) Multitenancy ASP.NET Core registration imports
     ```
     ```csharp
     // Step 4: (Begin) Register multitenancy services and ASP.NET Core adapter
     builder.Services.AddCleanArchitectureMultitenancy();
     builder.Services.AddCleanArchitectureMultitenancyAspNetCore(autoUseMiddleware: false);
     // Step 4: (End) Register multitenancy services and ASP.NET Core adapter
     ```
   - `samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution/src/Web/Program.cs`:
     ```csharp
     // Step 4: (Begin) Multitenancy middleware import
     using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Middleware;
     // Step 4: (End) Multitenancy middleware import
     ```
     ```csharp
     // Step 4: (Begin) Add multitenancy middleware between routing and auth
     app.UseRouting();
     app.UseCleanArchitectureMultitenancy();
     app.UseAuthentication();
     app.UseAuthorization();
     // Step 4: (End) Add multitenancy middleware between routing and auth
     ```
5. Add route conventions that group tenant-bound APIs under `/tenants/{tenantId}/...`; keep health/status endpoints outside the group for anonymous access.

   - `samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution/src/Web/Infrastructure/WebApplicationExtensions.cs`:

     ```csharp
     // Step 5: (Begin) Prefix tenant-bound endpoints with tenant route
     var tenantRoutePrefix = "/api/tenants/{tenantId}";

     var routeGroup = app
         .MapGroup($"{tenantRoutePrefix}/{groupName}")
         .WithGroupName(groupName)
         .WithTags(groupName);
     // Step 5: (End) Prefix tenant-bound endpoints with tenant route
     ```

6. Decorate tenant-bound endpoints with `RequireTenant`, and mark public endpoints with `AllowAnonymousTenant` to keep resolution optional without enforcement (requires `AllowAnonymous = true` in Step 3).
   - `samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution/src/Web/Infrastructure/WebApplicationExtensions.cs`:
     ```csharp
     // Step 6: (Begin) Tenant enforcement routing helpers
     using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Routing;
     // Step 6: (End) Tenant enforcement routing helpers
     ```
     ```csharp
     // Step 6: (Begin) Enforce tenant requirements for grouped endpoints
     routeGroup.AddTenantEnforcement();
     routeGroup.RequireTenant();
     // Step 6: (End) Enforce tenant requirements for grouped endpoints
     ```
   - `samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution/src/Web/Program.cs`:
     ```csharp
     // Step 6: (Begin) Tenant requirement routing helpers
     using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Routing;
     // Step 6: (End) Tenant requirement routing helpers
     ```
     ```csharp
     // Step 6: (Begin) Allow tenant-less access for public endpoints
     app.Map("/", () => Results.Redirect("/api"))
         .AddTenantEnforcement()
         .AllowAnonymousTenant();
     // Step 6: (End) Allow tenant-less access for public endpoints
     ```
7. Enable `TenantExceptionHandler`/ProblemDetails so unresolved tenants return 400, missing tenants return 404, and suspended tenants return 403.
   - `samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution/src/Web/DependencyInjection.cs`:
     ```csharp
     // Step 7: (Begin) Register ProblemDetails for exception handling
     builder.Services.AddProblemDetails();
     // Step 7: (End) Register ProblemDetails for exception handling
     ```
   - `samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution/src/Web/Program.cs`:
     ```csharp
     // Step 7: (Begin) Enable exception handlers for ProblemDetails responses
     app.UseExceptionHandler();
     // Step 7: (End) Enable exception handlers for ProblemDetails responses
     ```
8. Add integration tests that cover: resolved via route, resolved via host mapping, header fallback when the route is absent, conflict handling when route/header disagree, and enforcement responses when no tenant is provided.
9. Update the sample README with the walkthrough (inputs, expected status codes) and middleware ordering reminders.

## Validation

- Requests with `/tenants/{tenantId}` succeed only when the tenant exists and is active.
- Requests without tenant context return the expected ProblemDetails payloads.
- Tenant context is cleared per request (no AsyncLocal leakage between tests).
