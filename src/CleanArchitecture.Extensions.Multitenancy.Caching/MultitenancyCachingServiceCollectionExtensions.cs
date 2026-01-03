using CleanArchitecture.Extensions.Caching.Abstractions;
using CleanArchitecture.Extensions.Caching.Keys;
using CleanArchitecture.Extensions.Multitenancy.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CleanArchitecture.Extensions.Multitenancy;

/// <summary>
/// Registration helpers for multitenancy caching integration.
/// </summary>
public static class MultitenancyCachingServiceCollectionExtensions
{
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
}
