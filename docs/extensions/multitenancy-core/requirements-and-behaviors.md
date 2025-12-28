# Multitenancy Core: Requirements and behaviors

This page documents how tenant requirements are expressed and how pipeline behaviors enforce them.

## Tenant requirements

A request or endpoint can declare its requirement in two ways:

1) Implement `ITenantRequirement` on the request type.
2) Apply `RequiresTenantAttribute` or `AllowHostRequestsAttribute` on the request or endpoint.

Example request-level requirement:

```csharp
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using MediatR;

public sealed record GetBillingSummaryQuery() : IRequest<string>, ITenantRequirement
{
    public TenantRequirementMode Requirement => TenantRequirementMode.Required;
}
```

Optional host-level request:

```csharp
using CleanArchitecture.Extensions.Multitenancy;
using MediatR;

[AllowHostRequests]
public sealed record GetHealthQuery() : IRequest<string>;
```

If no requirement is specified, the system falls back to:
- `MultitenancyOptions.RequireTenantByDefault` (default true)
- `MultitenancyOptions.AllowAnonymous`

## Pipeline behaviors

The core package includes the following MediatR behaviors:

- `TenantValidationBehavior` - validates tenant metadata against cache/store.
- `TenantEnforcementBehavior` - enforces resolution and tenant lifecycle checks.
- `TenantCorrelationBehavior` - adds tenant ID to logging scope and activity baggage.
- `TenantScopedCacheBehavior` - warns when cache scope is missing tenant context.

Register them using the extension method:

```csharp
using CleanArchitecture.Extensions.Multitenancy.Behaviors;
using MediatR;

services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();

    // Template behaviors...
    // cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
    // cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));

    cfg.AddCleanArchitectureMultitenancyPipeline();
    cfg.AddOpenBehavior(typeof(TenantScopedCacheBehavior<,>)); // optional
});
```

Place the call near your other request checks so tenant enforcement happens before the handler executes.

## Enforcement rules

`TenantEnforcementBehavior` throws when:

- No tenant is resolved (`TenantNotResolvedException`).
- Tenant is not validated (`TenantNotFoundException`).
- Tenant is suspended (`TenantSuspendedException`).
- Tenant is inactive, soft-deleted, pending provisioning, deleted, or expired (`TenantInactiveException`).

If you need to bypass enforcement for specific endpoints, apply `AllowHostRequestsAttribute` or return `TenantRequirementMode.Optional`.

## Optional: map exceptions to HTTP responses

In ASP.NET Core, map tenant exceptions to HTTP status codes:

```csharp
using CleanArchitecture.Extensions.Multitenancy.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

app.UseExceptionHandler(handler =>
{
    handler.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var status = feature?.Error switch
        {
            TenantNotResolvedException => StatusCodes.Status400BadRequest,
            TenantNotFoundException => StatusCodes.Status404NotFound,
            TenantSuspendedException => StatusCodes.Status403Forbidden,
            TenantInactiveException => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.StatusCode = status;
        await context.Response.WriteAsync("Tenant error.");
    });
});
```
