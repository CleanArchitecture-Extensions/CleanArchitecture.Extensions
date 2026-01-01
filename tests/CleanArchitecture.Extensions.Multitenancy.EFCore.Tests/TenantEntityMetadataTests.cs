using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Filters;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Tests;

public class TenantEntityMetadataTests
{
    [Fact]
    public void IsGlobalEntity_detects_interface_and_attribute()
    {
        var options = new EfCoreMultitenancyOptions();

        Assert.True(TenantEntityMetadata.IsGlobalEntity(typeof(InterfaceGlobalEntity), options));
        Assert.True(TenantEntityMetadata.IsGlobalEntity(typeof(AttributeGlobalEntity), options));
    }

    [Fact]
    public void IsGlobalEntity_detects_configured_type_and_name()
    {
        var options = new EfCoreMultitenancyOptions();
        options.GlobalEntityTypes.Add(typeof(NamedGlobalEntity));
        options.GlobalEntityTypeNames.Add(typeof(FullNameGlobalEntity).FullName!);
        options.GlobalEntityTypeNames.Add(nameof(SimpleNameGlobalEntity));

        Assert.True(TenantEntityMetadata.IsGlobalEntity(typeof(NamedGlobalEntity), options));
        Assert.True(TenantEntityMetadata.IsGlobalEntity(typeof(FullNameGlobalEntity), options));
        Assert.True(TenantEntityMetadata.IsGlobalEntity(typeof(SimpleNameGlobalEntity), options));
    }

    [Fact]
    public void IsTenantScoped_excludes_owned_and_keyless_types()
    {
        using var dbContext = new MetadataDbContext(CreateOptions());
        var options = new EfCoreMultitenancyOptions();

        var ownedType = dbContext.Model.GetEntityTypes().Single(type => type.ClrType == typeof(OwnedWidget));
        var keylessType = dbContext.Model.FindEntityType(typeof(KeylessWidget));
        var regularType = dbContext.Model.FindEntityType(typeof(RegularWidget));

        Assert.NotNull(keylessType);
        Assert.NotNull(regularType);

        Assert.False(TenantEntityMetadata.IsTenantScoped(ownedType, options));
        Assert.False(TenantEntityMetadata.IsTenantScoped(keylessType!, options));
        Assert.True(TenantEntityMetadata.IsTenantScoped(regularType!, options));
    }

    private static DbContextOptions<MetadataDbContext> CreateOptions()
        => new DbContextOptionsBuilder<MetadataDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private sealed class MetadataDbContext : DbContext
    {
        public MetadataDbContext(DbContextOptions<MetadataDbContext> options)
            : base(options)
        {
        }

        public DbSet<RegularWidget> RegularWidgets => Set<RegularWidget>();

        public DbSet<KeylessWidget> KeylessWidgets => Set<KeylessWidget>();

        public DbSet<OwnerWidget> OwnerWidgets => Set<OwnerWidget>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KeylessWidget>().HasNoKey();
            modelBuilder.Entity<OwnerWidget>().OwnsOne(owner => owner.Owned);
        }
    }

    private sealed class InterfaceGlobalEntity : IGlobalEntity
    {
        public int Id { get; set; }
    }

    [GlobalEntity]
    private sealed class AttributeGlobalEntity
    {
        public int Id { get; set; }
    }

    private sealed class NamedGlobalEntity
    {
        public int Id { get; set; }
    }

    private sealed class FullNameGlobalEntity
    {
        public int Id { get; set; }
    }

    private sealed class SimpleNameGlobalEntity
    {
        public int Id { get; set; }
    }

    private sealed class RegularWidget
    {
        public int Id { get; set; }
    }

    private sealed class KeylessWidget
    {
        public string? Name { get; set; }
    }

    private sealed class OwnerWidget
    {
        public int Id { get; set; }
        public OwnedWidget Owned { get; set; } = new();
    }

    [Owned]
    private sealed class OwnedWidget
    {
        public string? Name { get; set; }
    }
}
