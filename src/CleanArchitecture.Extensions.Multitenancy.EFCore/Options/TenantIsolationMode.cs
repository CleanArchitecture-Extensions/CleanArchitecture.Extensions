namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Options;

/// <summary>
/// Defines the data isolation mode for tenant databases.
/// </summary>
public enum TenantIsolationMode
{
    /// <summary>
    /// Tenants share the same database and schema with row-level filtering.
    /// </summary>
    SharedDatabase,

    /// <summary>
    /// Tenants share the same database but use dedicated schemas.
    /// </summary>
    SchemaPerTenant,

    /// <summary>
    /// Tenants use dedicated databases.
    /// </summary>
    DatabasePerTenant
}
