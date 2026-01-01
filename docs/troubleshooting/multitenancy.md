# Troubleshooting: Multitenancy

Common multitenancy issues and fixes across core, ASP.NET Core, and EF Core adapters.

## TenantNotResolvedException

**Symptoms**

- Requests fail with "Tenant not resolved" or 400 ProblemDetails.

**Fixes**

- Ensure your host adapter populates `TenantResolutionContext` before handlers run.
- Verify `ResolutionOrder` includes the provider you expect (route/host/header/query/claim).
- Check that your route parameter name, header names, or claim type match the incoming request.

## TenantNotFoundException

**Symptoms**

- Requests fail even though a tenant ID is present.

**Fixes**

- Register `ITenantInfoStore` when using `TenantValidationMode.Repository`.
- Register `ITenantInfoCache` when using `TenantValidationMode.Cache`.
- Ensure the tenant exists in the store and is marked active.

## Ambiguous tenant candidates

**Symptoms**

- Tenant resolves as null even though IDs are present.

**Fixes**

- Ensure headers/queries contain only a single tenant ID.
- Disable `RequireMatchAcrossSources` or ensure all sources agree on the same ID.
- Avoid sending multiple `X-Tenant-ID` values separated by commas or semicolons.

## Host-based resolution returns null

**Symptoms**

- Host provider does not resolve tenant for subdomains.

**Fixes**

- Ensure requests include a subdomain (for example, `tenant.app.com`).
- Configure `HostTenantSelector` to support custom host patterns.
- Host provider ignores IP addresses and single-segment hosts.

## Cache scope mismatch warnings

**Symptoms**

- Logs show "Cache scope tenant mismatch".

**Fixes**

- Call `AddCleanArchitectureMultitenancyCaching` after `AddCleanArchitectureCaching`.
- Ensure tenant resolution runs before caching behaviors.

## Tenant context lost in background jobs

**Symptoms**

- Background workers execute without tenant context.

**Fixes**

- Serialize `TenantContext` with `ITenantContextSerializer` and restore it in the worker.
- Wrap work in `ITenantAccessor.BeginScope` to avoid AsyncLocal leakage.

## EF Core: TenantSaveChangesInterceptor errors

**Symptoms**

- Writes throw `TenantNotResolvedException` or a tenant mismatch exception.

**Fixes**

- Ensure tenant context is set before SaveChanges executes.
- Verify `TenantIdPropertyName` matches your model configuration.
- Confirm global entities are excluded with `IGlobalEntity` or `[GlobalEntity]`.

## EF Core: database-per-tenant errors

**Symptoms**

- `TenantDbContextFactory` throws "Tenant connection string was not resolved".

**Fixes**

- Configure `ConnectionStringFormat` or register a custom `ITenantConnectionResolver`.
- Ensure the current tenant is resolved before creating a DbContext.
- Use a relational EF Core provider (database-per-tenant requires relational).
