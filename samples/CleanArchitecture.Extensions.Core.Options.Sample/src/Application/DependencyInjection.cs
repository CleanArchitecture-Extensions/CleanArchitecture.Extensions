using System.Reflection;
using CleanArchitecture.Extensions.Core.Behaviors;
using CleanArchitecture.Extensions.Core.Guards;
using CleanArchitecture.Extensions.Core.Options;
using CleanArchitecture.Extensions.Core.Options.Sample.Application.Common.Behaviours;
using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Time;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAutoMapper(cfg =>
            cfg.AddMaps(Assembly.GetExecutingAssembly()));

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Services.Configure<CoreExtensionsOptions>(builder.Configuration.GetSection("Extensions:Core"));
        builder.Services.AddSingleton<IClock, SystemClock>();
        builder.Services.AddScoped<ILogContext, InMemoryLogContext>();
        builder.Services.AddScoped(typeof(IAppLogger<>), typeof(NoOpAppLogger<>));

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenRequestPreProcessor(typeof(LoggingPreProcessor<>));
            cfg.AddOpenBehavior(typeof(CorrelationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
        });

        builder.Services.AddSingleton(new GuardOptions
        {
            Strategy = GuardStrategy.ReturnFailure
        });
    }
}
