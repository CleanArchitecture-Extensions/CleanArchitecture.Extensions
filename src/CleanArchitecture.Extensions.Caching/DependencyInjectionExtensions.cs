using CleanArchitecture.Extensions.Caching.Abstractions;
using CleanArchitecture.Extensions.Caching.Adapters;
using CleanArchitecture.Extensions.Caching.Keys;
using CleanArchitecture.Extensions.Caching.Options;
using CleanArchitecture.Extensions.Caching.Serialization;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Caching;

/// <summary>
/// Registration helpers for CleanArchitecture.Extensions.Caching.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers caching options and default services.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Optional callback to configure <see cref="CachingOptions"/>.</param>
    /// <param name="configureQueryCaching">Optional callback to configure <see cref="QueryCachingBehaviorOptions"/>.</param>
    public static IServiceCollection AddCleanArchitectureCaching(
        this IServiceCollection services,
        Action<CachingOptions>? configure = null,
        Action<QueryCachingBehaviorOptions>? configureQueryCaching = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var optionsBuilder = services.AddOptions<CachingOptions>();
        if (configure is not null)
        {
            optionsBuilder.Configure(configure);
        }

        var queryOptionsBuilder = services.AddOptions<QueryCachingBehaviorOptions>();
        if (configureQueryCaching is not null)
        {
            queryOptionsBuilder.Configure(configureQueryCaching);
        }

        services.AddMemoryCache();
        services.AddDistributedMemoryCache();
        services.TryAddSingleton<TimeProvider>(TimeProvider.System);
        services.TryAddSingleton<ICacheSerializer, SystemTextJsonCacheSerializer>();
        services.TryAddSingleton<ICacheKeyFactory, DefaultCacheKeyFactory>();
        // Call AddCleanArchitectureMultitenancyCaching (Multitenancy.Caching package) to bind cache scopes to tenant context.
        services.TryAddScoped<ICacheScope, DefaultCacheScope>();
        services.TryAddSingleton<ICache>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<CachingOptions>>().Value;
            var serializers = sp.GetServices<ICacheSerializer>();
            var distributedCache = sp.GetService<IDistributedCache>();

            var useDistributed = options.Backend switch
            {
                CacheBackend.Distributed => true,
                CacheBackend.Memory => false,
                _ => distributedCache is not null && distributedCache is not MemoryDistributedCache
            };

            if (useDistributed)
            {
                if (distributedCache is null)
                {
                    throw new InvalidOperationException("CachingOptions.Backend is set to Distributed but no IDistributedCache is registered.");
                }

                return ActivatorUtilities.CreateInstance<DistributedCacheAdapter>(sp, serializers);
            }

            return ActivatorUtilities.CreateInstance<MemoryCacheAdapter>(sp, serializers);
        });
        services.TryAddSingleton<DistributedCacheAdapter>(sp =>
            ActivatorUtilities.CreateInstance<DistributedCacheAdapter>(sp, sp.GetServices<ICacheSerializer>()));

        return services;
    }

    /// <summary>
    /// Adds the MediatR caching-related behaviors to the pipeline (hooked up in later steps).
    /// </summary>
    /// <param name="configuration">MediatR service configuration.</param>
    public static MediatRServiceConfiguration AddCleanArchitectureCachingPipeline(
        this MediatRServiceConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        configuration.AddOpenBehavior(typeof(Behaviors.QueryCachingBehavior<,>));
        return configuration;
    }
}
