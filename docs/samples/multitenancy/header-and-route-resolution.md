# Scenario: Header + Route Resolution with ASP.NET Core Enforcement

## Goal
Document a sample that shows deterministic tenant resolution from route first, host second, and header fallback, with ProblemDetails responses when a tenant is missing or inactive.

## Sample name and location
- Solution: `CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution`
- Path: `samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution`

## Modules used
- Multitenancy core (resolution pipeline + behaviors)
- Multitenancy.AspNetCore (middleware, attributes, ProblemDetails)

## Prerequisites
- Generate the base Web API solution with the Clean Architecture template using SQLite (`dotnet new ca-sln -cf None ...`).
- Keep numbered step comments in code changes and mirror them in the sample README per repository guidance.

## Steps
1. Add project references to `CleanArchitecture.Extensions.Multitenancy` and `CleanArchitecture.Extensions.Multitenancy.AspNetCore` from `src/`, and include those projects in the sample solution.
2. Configure `MultitenancyOptions` for route-first ordering (`Route > Host > Header > Query > Claim`), set header name `X-Tenant-ID`, require tenants by default, and disable fallback tenants.
3. Register services with `AddCleanArchitectureMultitenancy` then `AddCleanArchitectureMultitenancyAspNetCore(autoUseMiddleware: false)`; place `UseCleanArchitectureMultitenancy` after routing and before authentication/authorization.
4. Add route conventions that group tenant-bound APIs under `/tenants/{tenantId}/...`; keep health/status endpoints outside the group for anonymous access.
5. Decorate tenant-bound endpoints with `RequireTenant`, and mark public endpoints with `AllowAnonymousTenant` to keep resolution optional without enforcement.
6. Enable `TenantExceptionHandler`/ProblemDetails so unresolved tenants return 400, missing tenants return 404, and suspended tenants return 403.
7. Add integration tests that cover: resolved via route, resolved via host mapping, header fallback when the route is absent, conflict handling when route/header disagree, and enforcement responses when no tenant is provided.
8. Update the sample README with the walkthrough (inputs, expected status codes) and middleware ordering reminders.

## Validation
- Requests with `/tenants/{tenantId}` succeed only when the tenant exists and is active.
- Requests without tenant context return the expected ProblemDetails payloads.
- Tenant context is cleared per request (no AsyncLocal leakage between tests).
