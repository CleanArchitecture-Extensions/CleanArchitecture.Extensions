using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Tests;

public class TenantModelCacheKeyFactoryTests
{
    [Fact]
    public void Factory_includes_schema_in_model_cache_key()
    {
        var services = new ServiceCollection();

        services.AddCleanArchitectureMultitenancyEfCore(options =>
        {
            options.Mode = TenantIsolationMode.SchemaPerTenant;
            options.SchemaNameFormat = "tenant_{0}";
            options.IncludeSchemaInModelCacheKey = true;
        });

        services.AddDbContext<TestTenantDbContext>((sp, options) =>
        {
            options.UseInMemoryDatabase("tenant-cache-key");
            options.UseTenantModelCacheKeyFactory(sp);
        });

        using var provider = services.BuildServiceProvider();

        object keyAlpha;
        using (var scope = provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TestTenantDbContext>();
            var factory = context.GetService<IModelCacheKeyFactory>();

            Assert.IsType<TenantModelCacheKeyFactory>(factory);

            context.CurrentTenantInfo = new TenantInfo("alpha");
            keyAlpha = factory.Create(context, designTime: false);
        }

        object keyBeta;
        using (var scope = provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TestTenantDbContext>();
            var factory = context.GetService<IModelCacheKeyFactory>();

            Assert.IsType<TenantModelCacheKeyFactory>(factory);

            context.CurrentTenantInfo = new TenantInfo("beta");
            keyBeta = factory.Create(context, designTime: false);
        }

        Assert.NotEqual(keyAlpha, keyBeta);
    }

    private sealed class TestTenantDbContext : DbContext, ITenantDbContext
    {
        public TestTenantDbContext(DbContextOptions<TestTenantDbContext> options)
            : base(options)
        {
        }

        public string? CurrentTenantId => CurrentTenantInfo?.TenantId;

        public ITenantInfo? CurrentTenantInfo { get; set; }
    }
}
