using System.Globalization;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Options;

/// <summary>
/// Configures EF Core multitenancy defaults for the extensions package.
/// </summary>
public sealed class EfCoreMultitenancyOptions
{
    private bool _useShadowTenantId = true;
    private bool _useShadowTenantIdSet;
    private bool _enableQueryFilters = true;
    private bool _enableQueryFiltersSet;
    private bool _enableSaveChangesEnforcement = true;
    private bool _enableSaveChangesEnforcementSet;

    /// <summary>
    /// Gets or sets the isolation mode used for tenant data.
    /// </summary>
    public TenantIsolationMode Mode { get; set; } = TenantIsolationMode.SharedDatabase;

    /// <summary>
    /// Gets or sets the tenant identifier property name.
    /// </summary>
    public string TenantIdPropertyName { get; set; } = "TenantId";

    /// <summary>
    /// Gets or sets a value indicating whether a shadow tenant identifier should be added when missing.
    /// Defaults to true for shared database mode; false for schema/database per-tenant modes.
    /// </summary>
    public bool UseShadowTenantId
    {
        get => _useShadowTenantIdSet ? _useShadowTenantId : RowLevelDefaultsEnabled;
        set
        {
            _useShadowTenantId = value;
            _useShadowTenantIdSet = true;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether query filters should be applied for tenant isolation.
    /// Defaults to true for shared database mode; false for schema/database per-tenant modes.
    /// </summary>
    public bool EnableQueryFilters
    {
        get => _enableQueryFiltersSet ? _enableQueryFilters : RowLevelDefaultsEnabled;
        set
        {
            _enableQueryFilters = value;
            _enableQueryFiltersSet = true;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether SaveChanges enforcement should run.
    /// Defaults to true for shared database mode; false for schema/database per-tenant modes.
    /// </summary>
    public bool EnableSaveChangesEnforcement
    {
        get => _enableSaveChangesEnforcementSet ? _enableSaveChangesEnforcement : RowLevelDefaultsEnabled;
        set
        {
            _enableSaveChangesEnforcement = value;
            _enableSaveChangesEnforcementSet = true;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether tenant context is required for write operations.
    /// </summary>
    public bool RequireTenantForWrites { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether schema names should be part of the model cache key.
    /// </summary>
    public bool IncludeSchemaInModelCacheKey { get; set; } = true;

    /// <summary>
    /// Gets or sets the default schema to use when no tenant is resolved.
    /// </summary>
    public string? DefaultSchema { get; set; }

    /// <summary>
    /// Gets or sets the schema name format string for schema-per-tenant mode.
    /// </summary>
    public string SchemaNameFormat { get; set; } = "tenant_{0}";

    /// <summary>
    /// Gets or sets the schema resolver callback for schema-per-tenant mode.
    /// </summary>
    public Func<ITenantInfo?, string?>? SchemaNameProvider { get; set; }

    /// <summary>
    /// Gets or sets the connection string format string for database-per-tenant mode.
    /// </summary>
    public string? ConnectionStringFormat { get; set; }

    /// <summary>
    /// Gets or sets the connection string resolver callback.
    /// </summary>
    public Func<ITenantInfo?, string?>? ConnectionStringProvider { get; set; }

    /// <summary>
    /// Gets the list of entity types excluded from tenant filtering.
    /// </summary>
    public ISet<Type> GlobalEntityTypes { get; } = new HashSet<Type>();

    /// <summary>
    /// Gets the list of entity type names excluded from tenant filtering.
    /// </summary>
    public ISet<string> GlobalEntityTypeNames { get; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the default options instance.
    /// </summary>
    public static EfCoreMultitenancyOptions Default => new();

    private bool RowLevelDefaultsEnabled => Mode == TenantIsolationMode.SharedDatabase;

    /// <summary>
    /// Resolves the schema name for the specified tenant.
    /// </summary>
    /// <param name="tenant">Tenant metadata.</param>
    public string? ResolveSchemaName(ITenantInfo? tenant)
    {
        if (SchemaNameProvider is not null)
        {
            return SchemaNameProvider(tenant);
        }

        if (!string.IsNullOrWhiteSpace(DefaultSchema) && tenant is null)
        {
            return DefaultSchema;
        }

        if (string.IsNullOrWhiteSpace(SchemaNameFormat) || tenant is null || string.IsNullOrWhiteSpace(tenant.TenantId))
        {
            return DefaultSchema;
        }

        return string.Format(CultureInfo.InvariantCulture, SchemaNameFormat, tenant.TenantId);
    }

    /// <summary>
    /// Resolves the connection string for the specified tenant.
    /// </summary>
    /// <param name="tenant">Tenant metadata.</param>
    public string? ResolveConnectionString(ITenantInfo? tenant)
    {
        if (ConnectionStringProvider is not null)
        {
            return ConnectionStringProvider(tenant);
        }

        if (string.IsNullOrWhiteSpace(ConnectionStringFormat) || tenant is null || string.IsNullOrWhiteSpace(tenant.TenantId))
        {
            return null;
        }

        return string.Format(CultureInfo.InvariantCulture, ConnectionStringFormat, tenant.TenantId);
    }
}
