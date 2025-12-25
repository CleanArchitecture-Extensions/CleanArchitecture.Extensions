using CleanArchitecture.Extensions.Core.DomainEvents;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CleanArchitecture.Extensions.Core;

/// <summary>
/// EF Core registration helpers for CleanArchitecture.Extensions.Core.
/// </summary>
public static class EntityFrameworkDependencyInjectionExtensions
{
    /// <summary>
    /// Registers the EF Core domain event interceptor for dispatching tracked events.
    /// </summary>
    public static IServiceCollection AddCleanArchitectureCoreEfCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<DispatchDomainEventsInterceptor>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>());

        return services;
    }
}
