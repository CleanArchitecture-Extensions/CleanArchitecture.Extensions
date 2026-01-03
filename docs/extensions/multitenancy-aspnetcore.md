# Extension: Multitenancy.AspNetCore

## Overview

CleanArchitecture.Extensions.Multitenancy.AspNetCore provides HTTP-specific adapters for the multitenancy core. It includes middleware to resolve tenants from `HttpContext`, endpoint filters for enforcement, and helpers for minimal APIs and MVC.

## When to use

- You need tenant resolution for HTTP requests.
- You want tenant enforcement in minimal APIs or MVC without custom filters.
- You want consistent ProblemDetails responses for multitenancy errors.

## Prereqs and compatibility

- Target framework: `net10.0`.
- Dependencies: `Microsoft.AspNetCore.App` and the multitenancy core package.

## Install

```powershell
dotnet add src/Web/Web.csproj package CleanArchitecture.Extensions.Multitenancy.AspNetCore
```

## Register services and middleware

```csharp
using CleanArchitecture.Extensions.Multitenancy.AspNetCore;

builder.Services.AddCleanArchitectureMultitenancyAspNetCore(autoUseMiddleware: true);

var app = builder.Build();
```

Use `autoUseMiddleware` when header or host resolution is enough. For claim- or route-based resolution, disable it and place `app.UseCleanArchitectureMultitenancy()` after authentication or routing.

If you prefer manual wiring, call `app.UseCleanArchitectureMultitenancy()` instead of enabling `autoUseMiddleware`.

## Minimal API enforcement

```csharp
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Routing;

app.MapGroup("/tenants/{tenantId}")
    .AddTenantEnforcement()
    .RequireTenant()
    .MapGet("/profile", () => Results.Ok());
```

## Tenant requirements for endpoints

- Minimal APIs: use `.RequireTenant()` or `.AllowAnonymousTenant()` on route groups or handlers.
- MVC: apply `[RequiresTenant]` or `[AllowAnonymousTenant]` to controllers/actions.

## MVC enforcement

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
    },
    aspNetOptions =>
    {
        aspNetOptions.CorrelationIdHeaderName = "X-Correlation-ID";
        aspNetOptions.StoreTenantInHttpContextItems = true;
    });
```

## Access the resolved tenant

If `StoreTenantInHttpContextItems` is enabled (default), you can read the resolved context:

```csharp
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Context;

var tenantContext = httpContext.GetTenantContext();
```

`GetTenantContext()` respects `AspNetCoreMultitenancyOptions.HttpContextItemKey` when options are registered.

## ProblemDetails mapping

```csharp
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.ProblemDetails;

if (TenantProblemDetailsMapper.TryCreate(exception, httpContext, out var details))
{
    return Results.Problem(details);
}
```

`AddCleanArchitectureMultitenancyAspNetCore` also registers `TenantExceptionHandler` so `UseExceptionHandler` can emit consistent ProblemDetails responses for multitenancy exceptions.

## Middleware ordering

- Place tenant resolution before authorization and handler execution.
- If you use claim-based tenant resolution, run authentication before `UseCleanArchitectureMultitenancy` so claims are available.
- Route-based resolution requires routing middleware before tenant resolution; prefer header/host when you cannot adjust the pipeline.

## Template defaults (JaysonTaylorCleanArchitectureBlank)

- Prefer header or host resolution to keep the template unchanged.
- If you opt into route-based tenancy, insert routing before the multitenancy middleware.

```csharp
app.UseRouting();
app.UseCleanArchitectureMultitenancy();
app.MapEndpoints();
```

## Key components

- `TenantResolutionMiddleware`
- `TenantEnforcementEndpointFilter` / `TenantEnforcementActionFilter`
- `EndpointConventionBuilderExtensions`
- `HttpContextTenantExtensions`
- `AspNetCoreMultitenancyOptions`
- `TenantExceptionHandler`

## Related docs

- [Multitenancy core](multitenancy-core.md)
- [Multitenancy options](../reference/multitenancy-options.md)
- [AspNetCore options](../reference/aspnetcore-multitenancy-options.md)
- [Troubleshooting](../troubleshooting/multitenancy.md)
