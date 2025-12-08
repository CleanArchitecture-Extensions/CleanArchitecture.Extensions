using CleanArchitecture.Extensions.Validation.Behaviors;
using CleanArchitecture.Extensions.Validation.Options;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Extensions.Validation;

/// <summary>
/// Registration helpers for CleanArchitecture.Extensions.Validation.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers validation options so consumers can configure strategy, logging, and error formatting.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Optional callback to configure <see cref="ValidationOptions"/>.</param>
    public static IServiceCollection AddCleanArchitectureValidation(
        this IServiceCollection services,
        Action<ValidationOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var optionsBuilder = services.AddOptions<ValidationOptions>();
        if (configure is not null)
        {
            optionsBuilder.Configure(configure);
        }

        return services;
    }

    /// <summary>
    /// Adds the MediatR validation behavior to the pipeline.
    /// </summary>
    /// <remarks>
    /// Register validators separately, for example with <c>services.AddValidatorsFromAssemblyContaining&lt;T&gt;()</c>.
    /// </remarks>
    public static MediatRServiceConfiguration AddCleanArchitectureValidationPipeline(
        this MediatRServiceConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        configuration.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        return configuration;
    }
}
