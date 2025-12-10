using CleanArchitecture.Extensions.Caching.Options;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Extensions.Caching;

/// <summary>
/// Registration helpers for CleanArchitecture.Extensions.Caching.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers caching options and placeholder services. Implementation details will be added in subsequent steps.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Optional callback to configure <see cref="CachingOptions"/>.</param>
    public static IServiceCollection AddCleanArchitectureCaching(
        this IServiceCollection services,
        Action<CachingOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var optionsBuilder = services.AddOptions<CachingOptions>();
        if (configure is not null)
        {
            optionsBuilder.Configure(configure);
        }

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

        return configuration;
    }
}
