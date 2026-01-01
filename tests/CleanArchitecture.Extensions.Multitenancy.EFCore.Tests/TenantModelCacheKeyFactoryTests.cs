using CleanArchitecture.Extensions.Multitenancy.Context;
using CleanArchitecture.Extensions.Multitenancy.EFCore;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Tests;

public class TenantModelCacheKeyFactoryTests
{
    [Fact]
    public void Create_includes_schema_name_for_schema_per_tenant()
    {
        var options = new EfCoreMultitenancyOptions
        {
            Mode = TenantIsolationMode.SchemaPerTenant,
            SchemaNameFormat = "tenant_{0}",
            IncludeSchemaInModelCacheKey = true
        };

        var accessorAlpha = new CurrentTenantAccessor();
        using var scopeAlpha = accessorAlpha.BeginScope(TestTenant.Create("alpha"));
        using var contextAlpha = TestDbContextFactory.Create(accessorAlpha, options);
        var dependencies = contextAlpha.GetService<ModelCacheKeyFactoryDependencies>();
        var factory = new TenantModelCacheKeyFactory(Microsoft.Extensions.Options.Options.Create(options), dependencies);
        var keyAlpha = factory.Create(contextAlpha, designTime: false);

        var accessorBeta = new CurrentTenantAccessor();
        using var scopeBeta = accessorBeta.BeginScope(TestTenant.Create("beta"));
        using var contextBeta = TestDbContextFactory.Create(accessorBeta, options);
        var keyBeta = factory.Create(contextBeta, designTime: false);

        Assert.NotEqual(keyAlpha, keyBeta);
    }

    [Fact]
    public void Create_returns_same_key_for_shared_mode()
    {
        var options = new EfCoreMultitenancyOptions
        {
            Mode = TenantIsolationMode.SharedDatabase,
            IncludeSchemaInModelCacheKey = true
        };

        var accessorAlpha = new CurrentTenantAccessor();
        using var scopeAlpha = accessorAlpha.BeginScope(TestTenant.Create("alpha"));
        using var contextAlpha = TestDbContextFactory.Create(accessorAlpha, options);
        var dependencies = contextAlpha.GetService<ModelCacheKeyFactoryDependencies>();
        var factory = new TenantModelCacheKeyFactory(Microsoft.Extensions.Options.Options.Create(options), dependencies);
        var keyAlpha = factory.Create(contextAlpha, designTime: false);

        var accessorBeta = new CurrentTenantAccessor();
        using var scopeBeta = accessorBeta.BeginScope(TestTenant.Create("beta"));
        using var contextBeta = TestDbContextFactory.Create(accessorBeta, options);
        var keyBeta = factory.Create(contextBeta, designTime: false);

        Assert.Equal(keyAlpha, keyBeta);
    }

    [Fact]
    public void Create_ignores_contexts_without_tenant_metadata()
    {
        var options = new EfCoreMultitenancyOptions
        {
            Mode = TenantIsolationMode.SchemaPerTenant,
            IncludeSchemaInModelCacheKey = true
        };

        using var context = new PlainDbContext(CreateOptions());
        var dependencies = context.GetService<ModelCacheKeyFactoryDependencies>();
        var factory = new TenantModelCacheKeyFactory(Microsoft.Extensions.Options.Options.Create(options), dependencies);

        var key = factory.Create(context, designTime: false);

        Assert.NotNull(key);
    }

    private static DbContextOptions<PlainDbContext> CreateOptions()
        => new DbContextOptionsBuilder<PlainDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private sealed class PlainDbContext : DbContext
    {
        public PlainDbContext(DbContextOptions<PlainDbContext> options)
            : base(options)
        {
        }
    }
}
