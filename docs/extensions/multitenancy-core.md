# Extension: Multitenancy Core

## Overview

CleanArchitecture.Extensions.Multitenancy provides the core tenant model, resolution pipeline, validation hooks, and MediatR behaviors without any ASP.NET Core or EF Core dependencies. It is host-agnostic: you supply a host adapter that builds a `TenantResolutionContext` and sets the current tenant.

## When to use

- You need consistent tenant resolution across APIs, background jobs, or message handlers.
- You want tenant enforcement at the pipeline level instead of scattering checks in handlers.
- You need tenant-aware logging and cache scoping.

## Prereqs and compatibility

- Target framework: `net10.0`.
- Dependencies: MediatR `13.1.0`, `Microsoft.Extensions.*` `10.0.0`.
- Host adapter required (use the ASP.NET Core adapter for web APIs).

## Install

```powershell
dotnet add src/Application/Application.csproj package CleanArchitecture.Extensions.Multitenancy
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Multitenancy
```

## Register services

```csharp
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Configuration;

builder.Services.AddCleanArchitectureMultitenancy(options =>
{
    options.HeaderNames = new[] { "X-Tenant-ID" };
    options.RouteParameterName = "tenantId";
    options.QueryParameterName = "tenantId";
    options.ClaimType = "tenant_id";
});
```

## Resolve and set tenant context (host adapter)

The core package does not read HTTP requests directly. A host adapter builds a `TenantResolutionContext` and calls `ITenantResolver`.

```csharp
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;

public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext httpContext, ITenantResolver resolver, ITenantAccessor accessor)
    {
        var context = new TenantResolutionContext
        {
            Host = httpContext.Request.Host.Host,
            CorrelationId = httpContext.TraceIdentifier
        };

        foreach (var header in httpContext.Request.Headers)
        {
            context.Headers[header.Key] = header.Value.ToString();
        }

        foreach (var route in httpContext.Request.RouteValues)
        {
            if (route.Value is not null)
            {
                context.RouteValues[route.Key] = route.Value.ToString()!;
            }
        }

        foreach (var query in httpContext.Request.Query)
        {
            context.Query[query.Key] = query.Value.ToString();
        }

        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            foreach (var claim in httpContext.User.Claims)
            {
                context.Claims[claim.Type] = claim.Value;
            }
        }

        var tenantContext = await resolver.ResolveAsync(context, httpContext.RequestAborted);
        using (accessor.BeginScope(tenantContext))
        {
            await _next(httpContext);
        }
    }
}
```

## Add MediatR behaviors

```csharp
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddCleanArchitectureMultitenancyCorrelationPreProcessor();
    cfg.AddCleanArchitectureMultitenancyPipeline();
});
```

If you use MediatR request logging pre-processors (template default), register `AddCleanArchitectureMultitenancyCorrelationPreProcessor` before logging so request logs include tenant context.
In the Jason Taylor template, keep the multitenancy pipeline after authorization behaviors so authorization runs first.

The pipeline includes:

- `TenantCorrelationBehavior` (adds tenant ID to logs and activity baggage)
- `TenantCorrelationPreProcessor` (adds tenant ID before request logging pre-processors)
- `TenantValidationBehavior` (optional validation against cache or store)
- `TenantEnforcementBehavior` (enforces resolution and lifecycle)

## Tenant requirements

Use `ITenantRequirement`, `RequiresTenantAttribute`, or `AllowHostRequestsAttribute` to control enforcement per request.

```csharp
public sealed record GetTenantSummaryQuery() : IRequest<string>, ITenantRequirement
{
    public TenantRequirementMode Requirement => TenantRequirementMode.Required;
}
```

## Validation and stores

Enable validation to prevent spoofed tenant IDs:

```csharp
builder.Services.Configure<MultitenancyOptions>(options =>
{
    options.ValidationMode = TenantValidationMode.Repository;
    options.ResolutionCacheTtl = TimeSpan.FromMinutes(10);
});
```

Implement `ITenantInfoStore` and (optional) `ITenantInfoCache` to back validation. When validation is enabled and no store/cache is registered, the system logs warnings and the tenant remains unvalidated.

## Caching integration

If you use the caching package, install the adapter and add the multitenancy caching scope so keys include `tenantId`:

```powershell
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Multitenancy.Caching
```

```csharp
builder.Services.AddCleanArchitectureCaching();
builder.Services.AddCleanArchitectureMultitenancyCaching();
```

## Context propagation

`CurrentTenantAccessor` uses `AsyncLocal`. Use `ITenantAccessor.BeginScope` for background jobs or message handlers, and `ITenantContextSerializer` when you need to serialize context into job payloads.

## Reference and deep dives

- [Resolution pipeline](multitenancy-core/resolution-pipeline.md)
- [Requirements and behaviors](multitenancy-core/requirements-and-behaviors.md)
- [Validation and stores](multitenancy-core/validation-and-stores.md)
- [Context propagation](multitenancy-core/context-propagation.md)
- [Caching integration](multitenancy-core/caching-integration.md)
- [Options reference](../reference/multitenancy-options.md)
- [Troubleshooting](../troubleshooting/multitenancy.md)

## Related modules

- [Multitenancy.AspNetCore](multitenancy-aspnetcore.md)
- [Multitenancy.EFCore](multitenancy-efcore.md)
- Multitenancy.Identity (planned)
- Multitenancy.Provisioning (planned)
