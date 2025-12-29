# Extension: Multitenancy.AspNetCore

## Overview
ASP.NET Core adapters for the multitenancy core package. This extension provides middleware to populate `TenantResolutionContext`, endpoint filters for tenant enforcement, and helpers for minimal APIs and MVC.

## When to use

- You need tenant resolution and enforcement for HTTP requests.
- You want minimal API helpers to mark tenant-required endpoints.
- You want consistent ProblemDetails responses for multitenancy errors.

## Prereqs & Compatibility

- Target frameworks: `net10.0`.
- Dependencies: `CleanArchitecture.Extensions.Multitenancy` and ASP.NET Core (`Microsoft.AspNetCore.App`).
- Host adapter: use the provided middleware to populate tenant context.

## Install

```bash
dotnet add src/YourWebProject/YourWebProject.csproj package CleanArchitecture.Extensions.Multitenancy.AspNetCore
```

## Usage

### Register services and middleware

```csharp
using CleanArchitecture.Extensions.Multitenancy.AspNetCore;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Middleware;

builder.Services.AddCleanArchitectureMultitenancyAspNetCore();

var app = builder.Build();
app.UseCleanArchitectureMultitenancy();
```

### Minimal API enforcement

```csharp
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Routing;

app.MapGroup("/tenants/{tenantId}")
    .AddTenantEnforcement()
    .RequireTenant()
    .MapGet("/profile", () => Results.Ok());
```

### MVC enforcement

```csharp
using CleanArchitecture.Extensions.Multitenancy.AspNetCore;

builder.Services
    .AddControllers()
    .AddMultitenancyEnforcement();
```

### Options

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

## ProblemDetails mapping

```csharp
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.ProblemDetails;

if (TenantProblemDetailsMapper.TryCreate(exception, httpContext, out var details))
{
    return Results.Problem(details);
}
```

## Pipeline ordering

Recommended middleware order for HTTP pipelines:

- Correlation
- Localization
- Multitenancy resolution
- Authentication
- Authorization
- MVC/minimal API handlers

## Key components

- `TenantResolutionMiddleware`
- `TenantEnforcementEndpointFilter` / `TenantEnforcementActionFilter`
- `EndpointConventionBuilderExtensions`
- `AspNetCoreMultitenancyOptions`
