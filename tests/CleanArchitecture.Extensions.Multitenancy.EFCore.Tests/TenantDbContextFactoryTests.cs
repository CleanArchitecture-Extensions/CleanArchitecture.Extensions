using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Tests;

public class TenantDbContextFactoryTests
{
    [Fact]
    public void CreateDbContext_sets_connection_string_for_database_per_tenant()
    {
        var services = new ServiceCollection();
        services.AddCleanArchitectureMultitenancy();
        services.AddCleanArchitectureMultitenancyEfCore(options =>
        {
            options.Mode = TenantIsolationMode.DatabasePerTenant;
            options.ConnectionStringFormat = "Data Source=tenant_{0}.db";
        });
        services.AddDbContextFactory<TestDbContext>(options =>
            options.UseSqlite("Data Source=default.db"));
        services.AddTenantDbContextFactory<TestDbContext>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var tenantAccessor = scope.ServiceProvider.GetRequiredService<ITenantAccessor>();
        using var tenantScope = tenantAccessor.BeginScope(TestTenant.Create("alpha"));

        var factory = scope.ServiceProvider.GetRequiredService<ITenantDbContextFactory<TestDbContext>>();
        using var context = factory.CreateDbContext();

        Assert.Equal("Data Source=tenant_alpha.db", context.Database.GetConnectionString());
    }

    [Fact]
    public void CreateDbContext_throws_when_connection_string_missing()
    {
        var services = new ServiceCollection();
        services.AddCleanArchitectureMultitenancy();
        services.AddCleanArchitectureMultitenancyEfCore(options =>
        {
            options.Mode = TenantIsolationMode.DatabasePerTenant;
        });
        services.AddDbContextFactory<TestDbContext>(options =>
            options.UseSqlite("Data Source=default.db"));
        services.AddTenantDbContextFactory<TestDbContext>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var tenantAccessor = scope.ServiceProvider.GetRequiredService<ITenantAccessor>();
        using var tenantScope = tenantAccessor.BeginScope(TestTenant.Create("alpha"));

        var factory = scope.ServiceProvider.GetRequiredService<ITenantDbContextFactory<TestDbContext>>();

        var exception = Assert.Throws<InvalidOperationException>(() => factory.CreateDbContext());
        Assert.Contains("No connection string resolved", exception.Message);
    }
}
