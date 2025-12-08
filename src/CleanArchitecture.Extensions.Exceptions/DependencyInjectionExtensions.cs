using CleanArchitecture.Extensions.Exceptions.Behaviors;
using CleanArchitecture.Extensions.Exceptions.Catalog;
using CleanArchitecture.Extensions.Exceptions.Options;
using CleanArchitecture.Extensions.Exceptions.Redaction;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CleanArchitecture.Extensions.Exceptions;

/// <summary>
/// Registration helpers for CleanArchitecture.Extensions.Exceptions.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers exception handling services (catalog, redactor, options).
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configureHandling">Optional callback to configure <see cref="ExceptionHandlingOptions"/>.</param>
    /// <param name="configureCatalog">Optional callback to configure <see cref="ExceptionCatalogOptions"/>.</param>
    public static IServiceCollection AddCleanArchitectureExceptions(
        this IServiceCollection services,
        Action<ExceptionHandlingOptions>? configureHandling = null,
        Action<ExceptionCatalogOptions>? configureCatalog = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var handlingBuilder = services.AddOptions<ExceptionHandlingOptions>();
        if (configureHandling is not null)
        {
            handlingBuilder.Configure(configureHandling);
        }

        var catalogBuilder = services.AddOptions<ExceptionCatalogOptions>();
        if (configureCatalog is not null)
        {
            catalogBuilder.Configure(configureCatalog);
        }

        services.TryAddSingleton<IExceptionCatalog, ExceptionCatalog>();
        services.TryAddSingleton<ExceptionRedactor>();

        return services;
    }

    /// <summary>
    /// Adds the MediatR exception wrapping behavior to the pipeline.
    /// </summary>
    public static MediatRServiceConfiguration AddCleanArchitectureExceptionsPipeline(
        this MediatRServiceConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        configuration.AddOpenBehavior(typeof(ExceptionWrappingBehavior<,>));
        return configuration;
    }
}
