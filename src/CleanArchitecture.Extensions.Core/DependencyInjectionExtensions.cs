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
    public static IServiceCollection AddCleanArchitectureCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<CoreExtensionsOptions>();
        services.TryAddSingleton<IClock, SystemClock>();
        services.TryAddScoped<ILogContext, InMemoryLogContext>();
        services.TryAddScoped(typeof(IAppLogger<>), typeof(MelAppLoggerAdapter<>));
        services.TryAddScoped<DomainEvents.DomainEventTracker>();
        services.TryAddScoped<DomainEvents.IDomainEventDispatcher, DomainEvents.MediatRDomainEventDispatcher>();

        return services;
    }

    /// <summary>
    /// Adds the recommended MediatR pipeline components in the same order as the Jason Taylor template, with correlation first.
    /// </summary>
    /// <remarks>
    /// Call this inside your MediatR registration: <c>services.AddMediatR(cfg => cfg.AddCleanArchitectureCorePipeline());</c>
    /// </remarks>
    public static MediatRServiceConfiguration AddCleanArchitectureCorePipeline(this MediatRServiceConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        configuration.AddOpenRequestPreProcessor(typeof(LoggingPreProcessor<>));
        configuration.AddOpenBehavior(typeof(CorrelationBehavior<,>));
        configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
        configuration.AddOpenBehavior(typeof(PerformanceBehavior<,>));
        return configuration;
    }
}
