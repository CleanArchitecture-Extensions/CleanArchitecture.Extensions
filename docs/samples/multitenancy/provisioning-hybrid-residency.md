# Scenario: Provisioning with Region-Aware Sharding and Dedicated Databases

## Goal
Document a sample that provisions tenants through an orchestrator, chooses shard/database based on region/plan, and runs migrations/seeds accordingly.

## Sample name and location
- Solution: `CleanArchitecture.Extensions.Samples.Multitenancy.ProvisioningHybrid`
- Path: `samples/CleanArchitecture.Extensions.Samples.Multitenancy.ProvisioningHybrid`

## Modules used
- Multitenancy core
- Multitenancy.EFCore
- Multitenancy.Provisioning (planned adapter)
- Multitenancy.Sharding (planned adapter)
- Multitenancy.AspNetCore for API hosting
- Multitenancy.Storage initializer for assets (optional)

## Prerequisites
- Base Web API solution using SQLite for shared mode, with the option to create per-tenant SQLite files for dedicated mode.
- Numbered step comments and matching README entries for every change.

## Steps
1. Reference core, AspNetCore, and EFCore packages; add `Multitenancy.Provisioning` and `Multitenancy.Sharding` once available, and include them in the sample solution.
2. Implement a tenant catalog store and register `ProvisioningOrchestrator` with the Requested -> Validating -> Provisioning -> Active -> Suspended -> Deleted state machine from the blueprint.
3. Configure `ITenantPlanBuilder` to choose isolation mode: default shared database, but premium/regulated tenants get a dedicated database on a region-tagged shard (via `IShardResolver`).
4. Hook `ITenantSchemaManager` to run EF Core migrations/seeds per tenant according to the selected isolation mode; provide a dry-run path for planning without execution.
5. Add API endpoints or CLI commands for create/suspend/delete tenant operations that call provisioning services and publish lifecycle events (`TenantCreated`, `TenantProvisioned`, `TenantSuspended`, `TenantDeleted`).
6. Integrate the storage initializer so tenant-specific folders/containers are created on activation, following the Multitenancy.Storage path conventions.
7. Add operational scripts/tests to simulate cold onboarding (pre-created database) and hybrid flows, verifying events fire in order and rollback works on partial failures.
8. Document monitoring expectations (progress logs per tenant, metrics for provisioning duration) and cleanup steps when deletion or suspension occurs.

## Validation
- Provisioning selects the correct isolation mode based on plan/region and records the choice in tenant metadata.
- Migrations/seeds run per tenant and can resume idempotently after failures.
- Lifecycle events drive downstream actions (Identity seeding, storage setup) without leaking tenant data across boundaries.
