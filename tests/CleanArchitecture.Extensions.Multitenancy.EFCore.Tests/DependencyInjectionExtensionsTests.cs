using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Interceptors;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Migrations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Tests;

public class DependencyInjectionExtensionsTests
{
    [Fact]
    public void AddCleanArchitectureMultitenancyEfCore_registers_services()
    {
        var services = new ServiceCollection();

        services.AddCleanArchitectureMultitenancyEfCore();

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(ITenantModelCustomizer));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(ITenantConnectionResolver));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(TenantSaveChangesInterceptor));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(ISaveChangesInterceptor));
    }

    [Fact]
    public void AddTenantDbContextFactory_registers_factory_and_runner()
    {
        var services = new ServiceCollection();

        services.AddTenantDbContextFactory<TestDbContext>();

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(ITenantDbContextFactory<TestDbContext>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(TenantMigrationRunner<TestDbContext>));
    }
}
