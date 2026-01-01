using CleanArchitecture.Extensions.Multitenancy.Context;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Tests;

public class TenantSaveChangesInterceptorTests
{
    [Fact]
    public void SavingChanges_sets_tenant_id_for_added_entities()
    {
        var accessor = new CurrentTenantAccessor();
        using var scope = accessor.BeginScope(TestTenant.Create("alpha"));

        var options = new EfCoreMultitenancyOptions
        {
            EnableSaveChangesEnforcement = true,
            RequireTenantForWrites = true
        };

        using var dbContext = TestDbContextFactory.Create(accessor, options, addInterceptor: true);
        var widget = new TenantWidget { Name = "Alpha", TenantId = "beta" };

        dbContext.TenantWidgets.Add(widget);
        dbContext.SaveChanges();

        Assert.Equal("alpha", widget.TenantId);
    }

    [Fact]
    public void SavingChanges_throws_on_cross_tenant_updates()
    {
        var databaseName = Guid.NewGuid().ToString();
        var options = new EfCoreMultitenancyOptions
        {
            EnableSaveChangesEnforcement = true,
            RequireTenantForWrites = true
        };

        var accessor = new CurrentTenantAccessor();
        using (accessor.BeginScope(TestTenant.Create("alpha")))
        {
            using var dbContext = TestDbContextFactory.Create(accessor, options, addInterceptor: true, databaseName: databaseName);
            dbContext.TenantWidgets.Add(new TenantWidget { Name = "Alpha" });
            dbContext.SaveChanges();
        }

        using (accessor.BeginScope(TestTenant.Create("beta")))
        {
            using var dbContext = TestDbContextFactory.Create(accessor, options, addInterceptor: true, databaseName: databaseName);
            var widget = new TenantWidget { Id = 1, TenantId = "alpha", Name = "Updated" };

            dbContext.Attach(widget);
            dbContext.Entry(widget).State = EntityState.Modified;

            Assert.Throws<InvalidOperationException>(() => dbContext.SaveChanges());
        }
    }
}
