# Scenario: Tenant Context Propagation into Background Jobs

## Goal
Document a sample that captures tenant context in HTTP requests and restores it inside background jobs so multi-tenant data access and logging stay isolated.

## Sample name and location
- Solution: `CleanArchitecture.Extensions.Samples.Multitenancy.BackgroundJobs`
- Path: `samples/CleanArchitecture.Extensions.Samples.Multitenancy.BackgroundJobs`

## Modules used
- Multitenancy core (context, serializer, enforcement behaviors)
- Multitenancy.AspNetCore (HTTP resolution)

## Prerequisites
- Create the base Web API solution using SQLite.
- Keep numbered step comments in code changes with matching entries in the sample README.

## Steps
1. Reference `CleanArchitecture.Extensions.Multitenancy` and `CleanArchitecture.Extensions.Multitenancy.AspNetCore`; enable middleware for header-based resolution and keep enforcement on.
2. Ensure `ITenantContextSerializer` is registered (from the core package) and configure correlation options if needed to include tenant IDs in log scopes.
3. Build an in-memory `IBackgroundJobQueue` abstraction that carries a payload plus serialized `TenantContext`.
4. In controllers/handlers that enqueue work, serialize the current `TenantContext` and attach it to the queued message.
5. Implement a hosted worker that dequeues messages, restores tenant context via `ITenantAccessor`/`ITenantContextSerializer`, runs the work, and clears `ICurrentTenant` afterward.
6. Register MediatR behaviors (`TenantEnforcementBehavior`, `TenantCorrelationBehavior`) so background commands also require a tenant when needed and emit tenant-aware log scopes.
7. Add tests to assert tenant context flows into the worker (log scope contains tenant ID; EF Core context sees the tenant) and that enqueuing without a tenant is rejected when the message type requires it.
8. Document failure handling: how to drop or poison messages when context deserialization fails and how to avoid leaking tenant info in those paths.

## Validation
- Jobs executed by the worker see the same tenant ID as the request that queued them.
- Messages created without tenant context are rejected for tenant-required workflows.
- Tenant context is cleared after each job to avoid cross-tenant leakage.
