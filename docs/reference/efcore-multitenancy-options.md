# Reference: EF Core multitenancy options

`EfCoreMultitenancyOptions` configures tenant isolation for EF Core.

## Options

| Option | Type | Default | Description |
| --- | --- | --- | --- |
| `Mode` | `TenantIsolationMode` | `SharedDatabase` | Shared database, schema-per-tenant, or database-per-tenant. |
| `TenantIdPropertyName` | `string` | `"TenantId"` | Name of the tenant ID property. |
| `UseShadowTenantId` | `bool` | `SharedDatabase: true; otherwise false` | Adds a shadow tenant ID when missing. |
| `EnableQueryFilters` | `bool` | `SharedDatabase: true; otherwise false` | Enables tenant query filters. |
| `EnableSaveChangesEnforcement` | `bool` | `SharedDatabase: true; otherwise false` | Enables SaveChanges enforcement. |
| `RequireTenantForWrites` | `bool` | `true` | Requires tenant context for writes. |
| `IncludeSchemaInModelCacheKey` | `bool` | `true` | Includes schema in model cache key for schema-per-tenant mode. |
| `DefaultSchema` | `string?` | `null` | Default schema when no tenant is resolved. |
| `SchemaNameFormat` | `string` | `"tenant_{0}"` | Schema format string for schema-per-tenant. |
| `SchemaNameProvider` | `Func<ITenantInfo?,string?>?` | `null` | Custom schema resolver (set in code). |
| `ConnectionStringFormat` | `string?` | `null` | Connection string format for database-per-tenant. |
| `ConnectionStringProvider` | `Func<ITenantInfo?,string?>?` | `null` | Custom connection resolver (set in code). |
| `GlobalEntityTypes` | `ISet<Type>` | empty | Entity types excluded from tenant filtering. |
| `GlobalEntityTypeNames` | `ISet<string>` | empty | Entity type names excluded from tenant filtering. |

## TenantIsolationMode

- `SharedDatabase`: row-level filtering within a shared database.
- `SchemaPerTenant`: shared database with per-tenant schema.
- `DatabasePerTenant`: dedicated database per tenant.

## Notes

- `SchemaNameProvider` and `ConnectionStringProvider` are delegates and must be set in code.
- For database-per-tenant, the DbContext factory sets the connection string per tenant and requires a relational provider.
- Row-level filtering/enforcement defaults to off for schema/database-per-tenant; set `UseShadowTenantId`, `EnableQueryFilters`, and `EnableSaveChangesEnforcement` to `true` to opt in.
