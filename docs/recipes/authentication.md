# Recipe: Authentication

## Goal
Wire authentication with extension-friendly hooks.

## Prereqs
- Base Clean Architecture template running.
- Auth provider chosen (e.g., JWT, IdentityServer) — placeholder until packages land.

## Steps
1. Add the relevant authentication adapter package (TBD).
2. Configure authentication in Program.cs (or equivalent) with provided helpers.
3. Add middleware/filters for tenant-aware auth if needed.

## Verify
- Hitting a protected endpoint returns 200 with valid token; 401 otherwise.

## Pitfalls
- Misaligned schemes between API and client; ensure defaults match.
- Ensure tenant resolution occurs before authz when multitenancy is enabled.
