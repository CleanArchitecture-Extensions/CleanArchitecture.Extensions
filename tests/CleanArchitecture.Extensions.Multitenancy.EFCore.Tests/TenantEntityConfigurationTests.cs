using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Tests;

public class TenantEntityConfigurationTests
{
    [Fact]
    public void Configure_adds_required_tenant_id_for_tenant_entities()
    {
        using var dbContext = new ConfigurationDbContext(CreateOptions());
        var entityType = dbContext.Model.FindEntityType(typeof(ConfigTenantEntity));

        Assert.NotNull(entityType);

        var property = entityType!.FindProperty("TenantId");

        Assert.NotNull(property);
        Assert.False(property!.IsNullable);
    }

    [Fact]
    public void Configure_adds_shadow_tenant_id_for_plain_entities()
    {
        using var dbContext = new ConfigurationDbContext(CreateOptions());
        var entityType = dbContext.Model.FindEntityType(typeof(ConfigPlainEntity));

        Assert.NotNull(entityType);

        var property = entityType!.FindProperty("TenantId");

        Assert.NotNull(property);
        Assert.True(property!.IsShadowProperty());
    }

    [Fact]
    public void Configure_skips_tenant_id_for_global_entities()
    {
        using var dbContext = new ConfigurationDbContext(CreateOptions());
        var entityType = dbContext.Model.FindEntityType(typeof(ConfigGlobalEntity));

        Assert.NotNull(entityType);

        var property = entityType!.FindProperty("TenantId");

        Assert.Null(property);
    }

    private static DbContextOptions<ConfigurationDbContext> CreateOptions()
        => new DbContextOptionsBuilder<ConfigurationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private sealed class ConfigurationDbContext : DbContext
    {
        public ConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ConfigTenantEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ConfigPlainEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ConfigGlobalEntityConfiguration());
        }
    }

    private sealed class ConfigTenantEntityConfiguration : TenantEntityConfiguration<ConfigTenantEntity>
    {
    }

    private sealed class ConfigPlainEntityConfiguration : TenantEntityConfiguration<ConfigPlainEntity>
    {
    }

    private sealed class ConfigGlobalEntityConfiguration : TenantEntityConfiguration<ConfigGlobalEntity>
    {
    }

    private sealed class ConfigTenantEntity : ITenantEntity
    {
        public int Id { get; set; }
        public string TenantId { get; set; } = string.Empty;
    }

    private sealed class ConfigPlainEntity
    {
        public int Id { get; set; }
    }

    private sealed class ConfigGlobalEntity : IGlobalEntity
    {
        public int Id { get; set; }
    }
}
