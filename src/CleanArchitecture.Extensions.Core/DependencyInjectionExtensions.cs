using CleanArchitecture.Extensions.Core.Behaviors;
using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Options;
using CleanArchitecture.Extensions.Core.Time;
using CleanArchitecture.Extensions.Core.DomainEvents;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CleanArchitecture.Extensions.Core;

/// <summary>
/// Registration helpers for wiring CleanArchitecture.Extensions.Core into the MediatR pipeline and DI container.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers default Core abstractions (clock, log context, logger adapter) and options.
    /// </summary>
    /// <remarks>
    /// EF Core interceptors are registered via CleanArchitecture.Extensions.Core.EFCore.
    /// </remarks>
    public static IServiceCollection AddCleanArchitectureCore(
        this IServiceCollection services,
        Action<CoreExtensionsOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var optionsBuilder = services.AddOptions<CoreExtensionsOptions>();
        if (configure is not null)
        {
            optionsBuilder.Configure(configure);
        }

        services.TryAddSingleton<IClock, SystemClock>();
        services.TryAddScoped<ILogContext, InMemoryLogContext>();
        services.TryAddScoped(typeof(IAppLogger<>), typeof(MelAppLoggerAdapter<>));
        services.TryAddScoped<DomainEvents.DomainEventTracker>();
        services.TryAddScoped<DomainEvents.IDomainEventDispatcher, DomainEvents.MediatRDomainEventDispatcher>();
        services.AddSingleton<Microsoft.Extensions.Options.IPostConfigureOptions<CoreExtensionsOptions>>(provider =>
            new DomainEvents.DomainEventTimePostConfigure(provider.GetRequiredService<IClock>()));
        services.AddSingleton(_ => DomainEvents.DomainEventTimeMarker.Instance);

        return services;
    }

    /// <summary>
    /// Adds the recommended MediatR pipeline components in the same order as the Jason Taylor template, with correlation first.
    /// </summary>
    /// <remarks>
    /// Call this inside your MediatR registration: <c>services.AddMediatR(cfg => cfg.AddCleanArchitectureCorePipeline());</c>
    /// </remarks>
    public static MediatRServiceConfiguration AddCleanArchitectureCorePipeline(
        this MediatRServiceConfiguration configuration,
        Action<CorePipelineOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new CorePipelineOptions();
        configure?.Invoke(options);

        if (options.UseLoggingPreProcessor)
        {
            configuration.AddOpenRequestPreProcessor(typeof(LoggingPreProcessor<>));
        }

        if (options.UseCorrelationBehavior)
        {
            configuration.AddOpenBehavior(typeof(CorrelationBehavior<,>));
        }

        if (options.UseLoggingBehavior)
        {
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
        }

        if (options.UsePerformanceBehavior)
        {
            configuration.AddOpenBehavior(typeof(PerformanceBehavior<,>));
        }

        return configuration;
    }
}
