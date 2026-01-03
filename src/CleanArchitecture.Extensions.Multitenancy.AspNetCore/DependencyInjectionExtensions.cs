using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Context;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Filters;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Middleware;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Options;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.ProblemDetails;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore;

/// <summary>
/// Registration helpers for CleanArchitecture.Extensions.Multitenancy.AspNetCore.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers multitenancy core services along with ASP.NET Core adapters.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configureCore">Optional callback to configure <see cref="MultitenancyOptions"/>.</param>
    /// <param name="configureAspNetCore">Optional callback to configure <see cref="AspNetCoreMultitenancyOptions"/>.</param>
    /// <param name="autoUseMiddleware">Whether to add the multitenancy middleware automatically.</param>
    public static IServiceCollection AddCleanArchitectureMultitenancyAspNetCore(
        this IServiceCollection services,
        Action<MultitenancyOptions>? configureCore = null,
        Action<AspNetCoreMultitenancyOptions>? configureAspNetCore = null,
        bool autoUseMiddleware = false)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddCleanArchitectureMultitenancy(configureCore);

        var optionsBuilder = services.AddOptions<AspNetCoreMultitenancyOptions>();
        if (configureAspNetCore is not null)
        {
            optionsBuilder.Configure(configureAspNetCore);
        }

        services.TryAddSingleton<ITenantResolutionContextFactory, DefaultTenantResolutionContextFactory>();
        services.TryAddScoped<TenantEnforcementEndpointFilter>();
        services.TryAddScoped<TenantEnforcementActionFilter>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IExceptionHandler, TenantExceptionHandler>());
        if (autoUseMiddleware)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IStartupFilter, TenantResolutionStartupFilter>());
        }

        return services;
    }

    /// <summary>
    /// Registers the MVC enforcement filter globally.
    /// </summary>
    /// <param name="builder">MVC builder.</param>
    public static IMvcBuilder AddMultitenancyEnforcement(this IMvcBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.TryAddScoped<TenantEnforcementActionFilter>();
        builder.Services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<TenantEnforcementActionFilter>();
        });

        return builder;
    }
}
