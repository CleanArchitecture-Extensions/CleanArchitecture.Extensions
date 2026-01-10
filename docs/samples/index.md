# Samples

There are still no runnable samples committed, but the multitenancy scenarios below capture the step-by-step work needed to add them under `samples/` when ready. Each plan follows the Clean Architecture template (SQLite by default) and the repository rule to keep numbered step comments mirrored in the sample README.

- [Header + route resolution with enforcement](multitenancy/header-and-route-resolution.md)
- [Tenant context propagation into background jobs](multitenancy/background-jobs-context-propagation.md)
- [EF Core database-per-tenant isolation](multitenancy/efcore-database-per-tenant.md)
- [Tenant-isolated Identity and authorization](multitenancy/identity-tenant-isolated-auth.md)
- [Provisioning with region-aware sharding and dedicated databases](multitenancy/provisioning-hybrid-residency.md)
- [Tenant-scoped Redis caching with key prefixing](multitenancy/redis-tenant-caching.md)
