# CleanArchitecture.Extensions.Multitenancy.AspNetCore

ASP.NET Core adapter for CleanArchitecture.Extensions.Multitenancy. It provides middleware to resolve tenants from HTTP requests, endpoint filters for enforcement, and helpers for minimal APIs and MVC.

## What you get

- Tenant resolution middleware that builds `TenantResolutionContext` from `HttpContext`.
- Minimal API endpoint filter (`TenantEnforcementEndpointFilter`).
- MVC action filter (`TenantEnforcementActionFilter`).
- Endpoint metadata helpers (`RequireTenant`, `AllowAnonymousTenant`, `WithTenantHeader`, `WithTenantRoute`).
- ProblemDetails mapping for multitenancy exceptions.
- Exception handler integration (`TenantExceptionHandler`) for consistent ProblemDetails responses.

## Install

```powershell
dotnet add package CleanArchitecture.Extensions.Multitenancy.AspNetCore
```

## Quickstart

### 1) Register services

```csharp
using CleanArchitecture.Extensions.Multitenancy.AspNetCore;

builder.Services.AddCleanArchitectureMultitenancyAspNetCore();
```

### 2) Add the middleware

```csharp
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Middleware;

app.UseCleanArchitectureMultitenancy();
```

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
- `AddCleanArchitectureMultitenancyAspNetCore` registers `TenantExceptionHandler` so `UseExceptionHandler` can map multitenancy exceptions to ProblemDetails.
