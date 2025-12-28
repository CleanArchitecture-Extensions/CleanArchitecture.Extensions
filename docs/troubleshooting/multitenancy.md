# Troubleshooting: Multitenancy Core

Common multitenancy issues and fixes.

## TenantNotResolvedException

Symptoms:
- Requests fail with "Tenant could not be resolved."

Fixes:
- Ensure your host adapter populates `TenantResolutionContext` before handlers run.
- Verify `ResolutionOrder` includes the provider you expect (route/host/header/query/claim).
- Check that your route parameter name, header names, or claim type match the incoming request.

## TenantNotFoundException

Symptoms:
- Requests fail even though a tenant ID is present.

Fixes:
- Register `ITenantInfoStore` (and optionally `ITenantInfoCache`) when using `TenantValidationMode.Repository`.
- Switch to `TenantValidationMode.None` temporarily to validate wiring.
- Ensure the tenant exists in the store and is marked active.

## Ambiguous tenant candidates

Symptoms:
- Tenant resolves as null even though IDs are present.

Fixes:
- Ensure headers/queries only contain a single tenant ID.
- Disable `RequireMatchAcrossSources` or ensure all sources agree on the same ID.
- Avoid sending multiple `X-Tenant-ID` values separated by commas/semicolons.

## Host-based resolution returns null

Symptoms:
- Host provider does not resolve tenant for subdomains.

Fixes:
- Ensure requests include a subdomain (for example, `tenant.app.com`).
- Configure `HostTenantSelector` to support custom host patterns.
- Host provider ignores IP addresses and single-segment hosts.

## Cache scope mismatch warnings

Symptoms:
- Logs show "Cache scope tenant mismatch."

Fixes:
- Call `AddCleanArchitectureMultitenancyCaching` after `AddCleanArchitectureCaching`.
- Ensure tenant context is set before caching behaviors run.

## Tenant context lost in background jobs

Symptoms:
- Background workers execute without tenant context.

Fixes:
- Serialize `TenantContext` with `ITenantContextSerializer` and restore it in the worker.
- Wrap work in `ITenantAccessor.BeginScope` to avoid AsyncLocal leakage.
