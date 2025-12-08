using System.Reflection;
using CleanArchitecture.Extensions.Core;
using CleanArchitecture.Extensions.Core.Guards;
using CleanArchitecture.Extensions.Core.Result.Sample.Application.Common.Behaviours;
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
            options.GuardStrategy = GuardStrategy.ReturnFailure;
        });

        builder.Services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddCleanArchitectureCorePipeline();
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });
    }
}
