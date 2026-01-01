using CleanArchitecture.Extensions.Multitenancy.Context;
using CleanArchitecture.Extensions.Multitenancy.EFCore;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;
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
}
