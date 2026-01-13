# CleanArchitecture.Extensions.Multitenancy.AspNetCore

ASP.NET Core adapter for CleanArchitecture.Extensions.Multitenancy. It provides middleware to resolve tenants from HTTP requests, endpoint filters for enforcement, and helpers for minimal APIs and MVC.

## What you get

- Tenant resolution middleware that builds `TenantResolutionContext` from `HttpContext`.
- Minimal API endpoint filter (`TenantEnforcementEndpointFilter`).
- MVC action filter (`TenantEnforcementActionFilter`).
- Endpoint metadata helpers (`RequireTenant`, `AllowAnonymousTenant`, `WithTenantHeader`, `WithTenantRoute`).
- ProblemDetails mapping for multitenancy exceptions.
- Exception handler integration (`TenantExceptionHandler`) for consistent ProblemDetails responses.
- Optional startup filters to auto-add the middleware and the default exception handler when you cannot modify `Program.cs`.

## Install

```powershell
dotnet add package CleanArchitecture.Extensions.Multitenancy.AspNetCore
```

## Quickstart

### 1) Register services

```csharp
using CleanArchitecture.Extensions.Multitenancy.AspNetCore;

builder.Services.AddCleanArchitectureMultitenancyAspNetCore(
    autoUseMiddleware: true,
    autoUseExceptionHandler: true); // default for the template
```

Use `autoUseMiddleware` when header or host resolution is enough and you do not depend on route values. For claim- or route-based resolution, disable it and place `app.UseCleanArchitectureMultitenancy()` after authentication or routing. `autoUseExceptionHandler` is enabled by default to work around the template's `UseExceptionHandler(options => { })` stub; disable it if you already call `app.UseExceptionHandler()` with the new IExceptionHandler pipeline intact.

### 2) Add the middleware (manual)

```csharp
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Middleware;

app.UseCleanArchitectureMultitenancy();
```

Skip this step when you enable `autoUseMiddleware`.

### 3) Enforce tenancy (minimal APIs)

```csharp
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Routing;

app.MapGroup("/tenants/{tenantId}")
    .AddTenantEnforcement()
    .RequireTenant()
    .MapGet("/profile", () => Results.Ok());
```

### 4) Enforce tenancy (MVC)

```csharp
using CleanArchitecture.Extensions.Multitenancy.AspNetCore;

builder.Services
    .AddControllers()
    .AddMultitenancyEnforcement();
```

## Options

```csharp
using CleanArchitecture.Extensions.Multitenancy.AspNetCore;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Options;
using CleanArchitecture.Extensions.Multitenancy.Configuration;

builder.Services.AddCleanArchitectureMultitenancyAspNetCore(
    autoUseExceptionHandler: true,
    coreOptions =>
    {
        coreOptions.HeaderNames = new[] { "X-Tenant-ID" };
        coreOptions.RouteParameterName = "tenantId";
        coreOptions.QueryParameterName = "tenantId";
    },
    aspNetOptions =>
    {
        aspNetOptions.CorrelationIdHeaderName = "X-Correlation-ID";
        aspNetOptions.StoreTenantInHttpContextItems = true;
    });
```

## Template defaults (JaysonTaylorCleanArchitectureBlank)

- Prefer header or host resolution to keep the template unchanged.
- Route-based tenancy requires routing middleware before multitenancy; opt in only if you can adjust the pipeline.

Example for route-based tenants (note the default exception handler already runs):

```csharp
app.UseRouting();
app.UseCleanArchitectureMultitenancy();
app.MapEndpoints();
```

## ProblemDetails mapping

Use `TenantProblemDetailsMapper` when you want to turn multitenancy exceptions into HTTP responses:

```csharp
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.ProblemDetails;

if (TenantProblemDetailsMapper.TryCreate(exception, httpContext, out var details))
{
    return Results.Problem(details);
}
```

## Notes

- This adapter depends on the core multitenancy package and wires its services automatically.
- MediatR behaviors still live in the core package (`AddCleanArchitectureMultitenancyPipeline`).
- The middleware stores the resolved `TenantContext` in `HttpContext.Items` by default.
- `GetTenantContext()` respects the configured `AspNetCoreMultitenancyOptions.HttpContextItemKey` when available.
- `AddCleanArchitectureMultitenancyAspNetCore` registers `TenantExceptionHandler` so `UseExceptionHandler` can map multitenancy exceptions to ProblemDetails.
- Keep `autoUseExceptionHandler` enabled (default) when the host pipeline does not call `UseExceptionHandler()` (or overrides it), so your registered `IExceptionHandler` implementations still run.
