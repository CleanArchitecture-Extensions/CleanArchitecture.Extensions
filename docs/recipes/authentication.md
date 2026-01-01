# Recipe: Authentication

## Goal

Wire authentication in a template-based solution while keeping extension integration clean.

## Prereqs

- Base Clean Architecture template running.
- Authentication strategy chosen (JWT, cookies, or Identity).

## Steps

1. Configure authentication schemes in `Program.cs` (or equivalent).
2. Add authorization policies for tenant-aware endpoints if needed.
3. If multitenancy is enabled, ensure tenant resolution and authentication are ordered correctly:
   - Use tenant resolution before authorization.
   - If tenant resolution depends on claims, run authentication before tenant resolution.

## Verify

- Hitting a protected endpoint returns `200` with a valid token and `401` without one.
- Tenant-required endpoints return a tenant error when the tenant is missing.

## Pitfalls

- Authentication runs after tenant resolution when claim-based resolution is enabled (claims will be empty).
- Mismatched schemes between API and client.
- Missing authorization policies on tenant-required endpoints.
