using System.Reflection;
using CleanArchitecture.Extensions.Core;
using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Application.Common.Behaviours;
using CleanArchitecture.Extensions.Core.Logging;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAutoMapper(cfg =>
            cfg.AddMaps(Assembly.GetExecutingAssembly()));

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Services.AddCleanArchitectureCore(options =>
        {
            options.CorrelationHeaderName = "X-Correlation-ID";
        });
        builder.Services.AddScoped<ILogContext, InMemoryLogContext>(); // keep in-memory context for sample logging

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddCleanArchitectureCorePipeline();
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });
    }
}
