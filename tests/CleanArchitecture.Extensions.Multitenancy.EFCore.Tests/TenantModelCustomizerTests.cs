using CleanArchitecture.Extensions.Multitenancy.Context;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;
using Microsoft.EntityFrameworkCore.Metadata;

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

        using var dbContext = TestDbContextFactory.Create(accessor, options);
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
}
