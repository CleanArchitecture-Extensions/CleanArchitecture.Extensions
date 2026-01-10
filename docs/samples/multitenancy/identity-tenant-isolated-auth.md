# Scenario: Tenant-Isolated Identity and Authorization

## Goal
Document a sample that ties Identity login and authorization to the current tenant using tenant-aware stores, claims, and policies.

## Sample name and location
- Solution: `CleanArchitecture.Extensions.Samples.Multitenancy.IdentityPerTenant`
- Path: `samples/CleanArchitecture.Extensions.Samples.Multitenancy.IdentityPerTenant`

## Modules used
- Multitenancy core
- Multitenancy.AspNetCore
- Multitenancy.Identity (planned adapter)
- Multitenancy.EFCore for data isolation

## Prerequisites
- Base Web API solution with Identity enabled (template default) and SQLite.
- Numbered step comments and matching README entries for all changes.

## Steps
1. Reference multitenancy core and AspNetCore; add `CleanArchitecture.Extensions.Multitenancy.Identity` when the package is available (use a project reference if built in-repo).
2. Ensure tenant resolution runs before authentication so login attempts already have tenant context (host/route/header providers configured).
3. Update Identity user and role entities/stores to implement `ITenantUser`/`ITenantRole`; scope user queries by tenant ID and namespace roles using the configured prefix pattern.
4. Register `TenantClaimsPrincipalFactory` to inject `tenant_id`, `tenant_name`, and per-tenant roles/permissions into JWTs or cookies.
5. Configure `TenantPolicyProvider` plus authorization handlers (`TenantMembershipHandler`, `TenantPermissionRequirement`) and swap existing policy registrations to tenant-aware versions.
6. Enforce tenant suspension/inactive flags during sign-in (fail fast with the proper error) and ensure issued tokens are invalidated if tenant state changes.
7. Add integration tests covering successful login within a tenant, rejected login for mismatched tenant, role prefixing behavior, and authorization policies that deny cross-tenant access.
8. Document how to seed a tenant admin user via provisioning events or startup seeding, and how to rotate keys without breaking tenant isolation.

## Validation
- Authentication only succeeds when the principal’s tenant matches the resolved tenant context.
- Authorization policies respect tenant-prefixed roles/permissions.
- Token claims include tenant identifiers and are rejected if the tenant is suspended or deleted.
