using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Factories;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Filters;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Interceptors;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Migrations;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore;

/// <summary>
/// Registration helpers for CleanArchitecture.Extensions.Multitenancy.EFCore.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers EF Core multitenancy services and defaults.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Optional callback to configure <see cref="EfCoreMultitenancyOptions"/>.</param>
    public static IServiceCollection AddCleanArchitectureMultitenancyEfCore(
        this IServiceCollection services,
        Action<EfCoreMultitenancyOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var optionsBuilder = services.AddOptions<EfCoreMultitenancyOptions>();
        if (configure is not null)
        {
            optionsBuilder.Configure(configure);
        }

        services.TryAddSingleton<ITenantModelCustomizer, TenantModelCustomizer>();
        services.TryAddSingleton<ITenantConnectionResolver, DefaultTenantConnectionResolver>();
        services.TryAddSingleton<IModelCacheKeyFactory, TenantModelCacheKeyFactory>();

        services.TryAddScoped<TenantSaveChangesInterceptor>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISaveChangesInterceptor, TenantSaveChangesInterceptor>());

        return services;
    }

    /// <summary>
    /// Registers the tenant-aware DbContext factory and migration runner.
    /// </summary>
    /// <typeparam name="TContext">DbContext type.</typeparam>
    /// <param name="services">Service collection.</param>
    public static IServiceCollection AddTenantDbContextFactory<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<ITenantDbContextFactory<TContext>, TenantDbContextFactory<TContext>>();
        services.TryAddScoped<TenantMigrationRunner<TContext>>();
        return services;
    }
}
