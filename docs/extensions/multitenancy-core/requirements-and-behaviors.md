# Multitenancy core: requirements and behaviors

This page explains how tenant requirements are expressed and how the MediatR behaviors enforce them.

## Tenant requirements

A request or endpoint can declare a requirement in two ways:

1) Implement `ITenantRequirement` on the request type.
2) Apply `RequiresTenantAttribute` or `AllowHostRequestsAttribute` to the request or endpoint.

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

If no requirement is specified, the default is determined by:

- `MultitenancyOptions.RequireTenantByDefault` (default `true`)
- `MultitenancyOptions.AllowAnonymous` (default `false`)

## Pipeline behaviors

Register the core behaviors with:

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
The correlation pre-processor registers a matching post-processor to clean up log scopes.

The pipeline includes:

- `TenantValidationBehavior` - validates tenant metadata against cache/store when enabled.
- `TenantEnforcementBehavior` - enforces resolution and lifecycle checks.
- `TenantCorrelationBehavior` - enriches logs and activity with tenant ID.
- `TenantCorrelationPreProcessor` + `TenantCorrelationPostProcessor` - enriches logs before request logging pre-processors and cleans up scopes.

`TenantScopedCacheBehavior` (from the Multitenancy.Caching package) is optional and warns when cache scope does not align with the current tenant.

## Enforcement rules

`TenantEnforcementBehavior` throws when:

- No tenant is resolved (`TenantNotResolvedException`).
- Tenant is not validated (`TenantNotFoundException`).
- Tenant is suspended (`TenantSuspendedException`).
- Tenant is inactive, soft-deleted, pending provisioning, deleted, or expired (`TenantInactiveException`).

These rules are evaluated against `TenantInfo` fields: `IsActive`, `IsSoftDeleted`, `State`, and `ExpiresAt`.

## Log and trace correlation

`TenantCorrelationBehavior` and `TenantCorrelationPreProcessor` add the tenant ID to:

- Log scopes (key is `MultitenancyOptions.LogScopeKey`)
- Activity tags and baggage when `AddTenantToActivity` is enabled

`TenantCorrelationPostProcessor` cleans up the log scope when the pre-processor is used.

## HTTP enforcement

For HTTP endpoints, use the ASP.NET Core adapter's filters (`TenantEnforcementEndpointFilter` or `TenantEnforcementActionFilter`). They apply the same enforcement rules and can map exceptions to ProblemDetails responses.
