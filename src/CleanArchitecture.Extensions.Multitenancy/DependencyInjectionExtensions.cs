using CleanArchitecture.Extensions.Caching.Abstractions;
using CleanArchitecture.Extensions.Caching.Keys;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Behaviors;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using CleanArchitecture.Extensions.Multitenancy.Context;
using CleanArchitecture.Extensions.Multitenancy.Providers;
using CleanArchitecture.Extensions.Multitenancy.Serialization;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CleanArchitecture.Extensions.Multitenancy;

/// <summary>
/// Registration helpers for CleanArchitecture.Extensions.Multitenancy.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers multitenancy services and default providers.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Optional callback to configure <see cref="MultitenancyOptions"/>.</param>
    public static IServiceCollection AddCleanArchitectureMultitenancy(
        this IServiceCollection services,
        Action<MultitenancyOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var optionsBuilder = services.AddOptions<MultitenancyOptions>();
        if (configure is not null)
        {
            optionsBuilder.Configure(configure);
        }

        services.TryAddSingleton<CurrentTenantAccessor>();
        services.TryAddSingleton<ITenantAccessor>(sp => sp.GetRequiredService<CurrentTenantAccessor>());
        services.TryAddSingleton<ICurrentTenant>(sp => sp.GetRequiredService<CurrentTenantAccessor>());
        services.TryAddSingleton<ITenantContextSerializer, SystemTextJsonTenantContextSerializer>();

        services.TryAddSingleton<ITenantResolutionStrategy, CompositeTenantResolutionStrategy>();
        services.TryAddScoped<ITenantResolver, TenantResolver>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ITenantProvider, RouteTenantProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ITenantProvider, HostTenantProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ITenantProvider, HeaderTenantProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ITenantProvider, QueryTenantProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ITenantProvider, ClaimTenantProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ITenantProvider, DefaultTenantProvider>());

        return services;
    }

    /// <summary>
    /// Registers the tenant-aware cache scope when caching services are available.
    /// </summary>
    /// <param name="services">Service collection.</param>
    public static IServiceCollection AddCleanArchitectureMultitenancyCaching(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (!services.Any(descriptor => descriptor.ServiceType == typeof(ICacheKeyFactory)))
        {
            throw new InvalidOperationException("Caching services must be registered before enabling multitenancy cache integration.");
        }

        services.Replace(ServiceDescriptor.Scoped<ICacheScope, TenantCacheScope>());
        return services;
    }

    /// <summary>
    /// Adds the multitenancy MediatR behaviors to the pipeline.
    /// </summary>
    /// <param name="configuration">MediatR service configuration.</param>
    public static MediatRServiceConfiguration AddCleanArchitectureMultitenancyPipeline(
        this MediatRServiceConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        configuration.AddOpenBehavior(typeof(TenantValidationBehavior<,>));
        configuration.AddOpenBehavior(typeof(TenantEnforcementBehavior<,>));
        configuration.AddOpenBehavior(typeof(TenantCorrelationBehavior<,>));
        return configuration;
    }
}
