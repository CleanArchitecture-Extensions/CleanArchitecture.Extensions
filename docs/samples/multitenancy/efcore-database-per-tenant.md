# Scenario: EF Core Database-Per-Tenant Isolation

## Goal
Document a sample that uses database-per-tenant isolation with EF Core helpers, per-tenant migrations, and strict SaveChanges enforcement.

## Sample name and location
- Solution: `CleanArchitecture.Extensions.Samples.Multitenancy.EfCoreDatabasePerTenant`
- Path: `samples/CleanArchitecture.Extensions.Samples.Multitenancy.EfCoreDatabasePerTenant`

## Modules used
- Multitenancy core
- Multitenancy.AspNetCore
- Multitenancy.EFCore

## Prerequisites
- Base Web API solution using SQLite with the Clean Architecture template.
- Numbered step comments and matching README entries for every change.

## Steps
1. Add references to `CleanArchitecture.Extensions.Multitenancy`, `CleanArchitecture.Extensions.Multitenancy.AspNetCore`, and `CleanArchitecture.Extensions.Multitenancy.EFCore`; include them in the sample solution.
2. Configure `EfCoreMultitenancyOptions` to `DatabasePerTenant`, set the tenant ID property name, and mark global entities (Identity tables, etc.) as exclusions.
3. Implement `ITenantConnectionResolver` mapping each tenant to a SQLite file under `App_Data/{tenantId}.db`, allowing premium/regional tenants to pick dedicated databases if needed.
4. Replace the default DbContext registration with `ITenantDbContextFactory<ApplicationDbContext>` using the `TenantDbContext` base class plus `TenantSaveChangesInterceptor` and the tenant-aware model customizer.
5. Wire `TenantMigrationRunner` to run migrations per tenant; include a CLI/hosted command that iterates the tenant catalog and migrates active tenants.
6. Ensure the web pipeline resolves tenants before EF Core usage and keep `TenantEnforcementBehavior` enabled so data access cannot occur without tenant context.
7. Add tests verifying per-tenant database files are created, cross-tenant queries are blocked, and global entities bypass filters when configured.
8. Document operational guidance: backup/restore per tenant, handling connection string rotation, and cleaning up databases on tenant deletion.

## Validation
- Each tenant writes to its own SQLite file; queries do not cross boundaries.
- SaveChanges fails when tenant context is missing or mismatched.
- Migration runner reports per-tenant status and supports reruns without data loss.
