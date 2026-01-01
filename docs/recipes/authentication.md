# Recipe: Authentication

## Goal
Wire authentication with extension-friendly hooks.

## Prereqs
- Base Clean Architecture template running.
- Auth provider chosen (e.g., JWT, cookies, Identity) using the template defaults.

## Steps
1. Configure authentication in Program.cs (or equivalent) using the template guidance.
2. If multitenancy is enabled, ensure tenant resolution runs before authorization checks.
3. Add tenant-aware authorization policies if needed.

## Verify
- Hitting a protected endpoint returns 200 with valid token; 401 otherwise.

## Pitfalls
- Misaligned schemes between API and client; ensure defaults match.
- Ensure tenant resolution occurs before authz when multitenancy is enabled.
