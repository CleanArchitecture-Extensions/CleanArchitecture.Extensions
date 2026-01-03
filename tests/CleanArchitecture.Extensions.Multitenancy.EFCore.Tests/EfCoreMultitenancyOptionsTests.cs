using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Tests;

public class EfCoreMultitenancyOptionsTests
{
    [Fact]
    public void Defaults_enable_row_level_for_shared_database()
    {
        var options = new EfCoreMultitenancyOptions
        {
            Mode = TenantIsolationMode.SharedDatabase
        };

        Assert.True(options.UseShadowTenantId);
        Assert.True(options.EnableQueryFilters);
        Assert.True(options.EnableSaveChangesEnforcement);
    }

    [Fact]
    public void Defaults_disable_row_level_for_schema_per_tenant()
    {
        var options = new EfCoreMultitenancyOptions
        {
            Mode = TenantIsolationMode.SchemaPerTenant
        };

        Assert.False(options.UseShadowTenantId);
        Assert.False(options.EnableQueryFilters);
        Assert.False(options.EnableSaveChangesEnforcement);
    }

    [Fact]
    public void Defaults_treat_identity_entities_as_global()
    {
        var options = new EfCoreMultitenancyOptions();

        Assert.True(options.TreatIdentityEntitiesAsGlobal);
    }

    [Fact]
    public void Explicit_row_level_settings_override_mode_defaults()
    {
        var options = new EfCoreMultitenancyOptions
        {
            Mode = TenantIsolationMode.DatabasePerTenant,
            UseShadowTenantId = true,
            EnableQueryFilters = true,
            EnableSaveChangesEnforcement = true
        };

        Assert.True(options.UseShadowTenantId);
        Assert.True(options.EnableQueryFilters);
        Assert.True(options.EnableSaveChangesEnforcement);
    }

    [Fact]
    public void ResolveSchemaName_uses_provider_when_configured()
    {
        var options = new EfCoreMultitenancyOptions
        {
            SchemaNameProvider = _ => "custom"
        };

        var schema = options.ResolveSchemaName(new TenantInfo("alpha"));

        Assert.Equal("custom", schema);
    }

    [Fact]
    public void ResolveSchemaName_uses_default_schema_for_null_tenant()
    {
        var options = new EfCoreMultitenancyOptions
        {
            DefaultSchema = "default"
        };

        var schema = options.ResolveSchemaName(null);

        Assert.Equal("default", schema);
    }

    [Fact]
    public void ResolveSchemaName_formats_schema_for_tenant()
    {
        var options = new EfCoreMultitenancyOptions
        {
            SchemaNameFormat = "tenant_{0}"
        };

        var schema = options.ResolveSchemaName(new TenantInfo("alpha"));

        Assert.Equal("tenant_alpha", schema);
    }

    [Fact]
    public void ResolveSchemaName_falls_back_to_default_when_format_missing()
    {
        var options = new EfCoreMultitenancyOptions
        {
            SchemaNameFormat = string.Empty,
            DefaultSchema = "fallback"
        };

        var schema = options.ResolveSchemaName(new TenantInfo("alpha"));

        Assert.Equal("fallback", schema);
    }

    [Fact]
    public void ResolveConnectionString_uses_provider_when_configured()
    {
        var options = new EfCoreMultitenancyOptions
        {
            ConnectionStringProvider = _ => "provider"
        };

        var connectionString = options.ResolveConnectionString(new TenantInfo("alpha"));

        Assert.Equal("provider", connectionString);
    }

    [Fact]
    public void ResolveConnectionString_formats_string_for_tenant()
    {
        var options = new EfCoreMultitenancyOptions
        {
            ConnectionStringFormat = "Server={0};"
        };

        var connectionString = options.ResolveConnectionString(new TenantInfo("alpha"));

        Assert.Equal("Server=alpha;", connectionString);
    }

    [Fact]
    public void ResolveConnectionString_returns_null_for_missing_tenant()
    {
        var options = new EfCoreMultitenancyOptions
        {
            ConnectionStringFormat = "Server={0};"
        };

        var connectionString = options.ResolveConnectionString(null);

        Assert.Null(connectionString);
    }
}
