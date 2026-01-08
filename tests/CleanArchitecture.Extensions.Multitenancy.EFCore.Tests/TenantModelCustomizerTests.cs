using CleanArchitecture.Extensions.Multitenancy.Context;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Filters;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Tests;

public class TenantModelCustomizerTests
{
    [Fact]
    public void Customize_adds_shadow_property_and_filter_for_tenant_entities()
    {
        var accessor = new CurrentTenantAccessor();
        using var scope = accessor.BeginScope(TestTenant.Create("alpha"));

        var options = new EfCoreMultitenancyOptions
        {
            UseShadowTenantId = true,
            EnableQueryFilters = true
        };

        using var dbContext = NoShadowTestDbContextFactory.Create(accessor, options);
        var entityType = dbContext.Model.FindEntityType(typeof(ShadowWidget));

        Assert.NotNull(entityType);
        var property = entityType!.FindProperty(options.TenantIdPropertyName);

        Assert.NotNull(property);
        Assert.True(property!.IsShadowProperty());
        Assert.NotEmpty(entityType.GetDeclaredQueryFilters());
    }

    [Fact]
    public void Customize_skips_global_entities()
    {
        var accessor = new CurrentTenantAccessor();
        using var scope = accessor.BeginScope(TestTenant.Create("alpha"));

        var options = new EfCoreMultitenancyOptions
        {
            UseShadowTenantId = true,
            EnableQueryFilters = true
        };

        using var dbContext = TestDbContextFactory.Create(accessor, options);
        var entityType = dbContext.Model.FindEntityType(typeof(GlobalWidget));

        Assert.NotNull(entityType);
        Assert.Empty(entityType!.GetDeclaredQueryFilters());
    }

    [Fact]
    public void Customize_throws_when_tenant_property_missing_in_shared_database()
    {
        var accessor = new CurrentTenantAccessor();
        using var scope = accessor.BeginScope(TestTenant.Create("alpha"));

        var options = new EfCoreMultitenancyOptions
        {
            UseShadowTenantId = false,
            EnableQueryFilters = true
        };

        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            using var dbContext = ShadowDisabledTestDbContextFactory.Create(accessor, options);
            _ = dbContext.Model.FindEntityType(typeof(ShadowWidget));
        });

        Assert.Contains("Tenant property", exception.Message);
    }

    [Fact]
    public void Customize_skips_filters_when_disabled()
    {
        var accessor = new CurrentTenantAccessor();
        using var scope = accessor.BeginScope(TestTenant.Create("alpha"));

        var options = new EfCoreMultitenancyOptions
        {
            UseShadowTenantId = true,
            EnableQueryFilters = false
        };

        using var dbContext = NoFilterTestDbContextFactory.Create(accessor, options);
        var entityType = dbContext.Model.FindEntityType(typeof(TenantWidget));

        Assert.NotNull(entityType);
        Assert.Empty(entityType!.GetDeclaredQueryFilters());
    }

    [Fact]
    public void Customize_combines_with_existing_filters()
    {
        var accessor = new CurrentTenantAccessor();
        using var scope = accessor.BeginScope(TestTenant.Create("alpha"));

        var options = new EfCoreMultitenancyOptions
        {
            UseShadowTenantId = true,
            EnableQueryFilters = true
        };

        using var dbContext = FilteredTestDbContextFactory.Create(accessor, options);

        dbContext.TenantWidgets.AddRange(
            new TenantWidget { Name = "Visible", TenantId = "alpha" },
            new TenantWidget { Name = "Hidden", TenantId = "alpha" },
            new TenantWidget { Name = "Visible", TenantId = "beta" });

        dbContext.SaveChanges();

        var results = dbContext.TenantWidgets.ToList();

        Assert.Single(results);
        Assert.Equal("Visible", results[0].Name);
        Assert.Equal("alpha", results[0].TenantId);
    }

    private sealed class FilteredTestDbContext : TenantDbContext
    {
        public FilteredTestDbContext(
            DbContextOptions<FilteredTestDbContext> options,
            ICurrentTenant currentTenant,
            IOptions<EfCoreMultitenancyOptions> optionsAccessor,
            ITenantModelCustomizer modelCustomizer)
            : base(options, currentTenant, optionsAccessor, modelCustomizer)
        {
        }

        public DbSet<TenantWidget> TenantWidgets => Set<TenantWidget>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TenantWidget>()
                .HasQueryFilter(widget => widget.Name != "Hidden");

            ApplyTenantModel(modelBuilder);
        }
    }

    private static class FilteredTestDbContextFactory
    {
        public static FilteredTestDbContext Create(CurrentTenantAccessor accessor, EfCoreMultitenancyOptions options)
        {
            var builder = new DbContextOptionsBuilder<FilteredTestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString());

            return new FilteredTestDbContext(
                builder.Options,
                accessor,
                Microsoft.Extensions.Options.Options.Create(options),
                new TenantModelCustomizer());
        }
    }

    private sealed class NoShadowTestDbContext : TenantDbContext
    {
        public NoShadowTestDbContext(
            DbContextOptions<NoShadowTestDbContext> options,
            ICurrentTenant currentTenant,
            IOptions<EfCoreMultitenancyOptions> optionsAccessor,
            ITenantModelCustomizer modelCustomizer)
            : base(options, currentTenant, optionsAccessor, modelCustomizer)
        {
        }

        public DbSet<ShadowWidget> ShadowWidgets => Set<ShadowWidget>();
    }

    private static class NoShadowTestDbContextFactory
    {
        public static NoShadowTestDbContext Create(CurrentTenantAccessor accessor, EfCoreMultitenancyOptions options)
        {
            var builder = new DbContextOptionsBuilder<NoShadowTestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString());

            return new NoShadowTestDbContext(
                builder.Options,
                accessor,
                Microsoft.Extensions.Options.Options.Create(options),
                new TenantModelCustomizer());
        }
    }

    private sealed class ShadowDisabledTestDbContext : TenantDbContext
    {
        public ShadowDisabledTestDbContext(
            DbContextOptions<ShadowDisabledTestDbContext> options,
            ICurrentTenant currentTenant,
            IOptions<EfCoreMultitenancyOptions> optionsAccessor,
            ITenantModelCustomizer modelCustomizer)
            : base(options, currentTenant, optionsAccessor, modelCustomizer)
        {
        }

        public DbSet<ShadowWidget> ShadowWidgets => Set<ShadowWidget>();
    }

    private static class ShadowDisabledTestDbContextFactory
    {
        public static ShadowDisabledTestDbContext Create(CurrentTenantAccessor accessor, EfCoreMultitenancyOptions options)
        {
            var builder = new DbContextOptionsBuilder<ShadowDisabledTestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString());

            return new ShadowDisabledTestDbContext(
                builder.Options,
                accessor,
                Microsoft.Extensions.Options.Options.Create(options),
                new TenantModelCustomizer());
        }
    }

    private sealed class NoFilterTestDbContext : TenantDbContext
    {
        public NoFilterTestDbContext(
            DbContextOptions<NoFilterTestDbContext> options,
            ICurrentTenant currentTenant,
            IOptions<EfCoreMultitenancyOptions> optionsAccessor,
            ITenantModelCustomizer modelCustomizer)
            : base(options, currentTenant, optionsAccessor, modelCustomizer)
        {
        }

        public DbSet<TenantWidget> TenantWidgets => Set<TenantWidget>();
    }

    private static class NoFilterTestDbContextFactory
    {
        public static NoFilterTestDbContext Create(CurrentTenantAccessor accessor, EfCoreMultitenancyOptions options)
        {
            var builder = new DbContextOptionsBuilder<NoFilterTestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString());

            return new NoFilterTestDbContext(
                builder.Options,
                accessor,
                Microsoft.Extensions.Options.Options.Create(options),
                new TenantModelCustomizer());
        }
    }
}
