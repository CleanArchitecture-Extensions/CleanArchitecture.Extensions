using System.Reflection;
using CleanArchitecture.Extensions.Core.Logging.Sample.Application.Common.Behaviours;
using CleanArchitecture.Extensions.Core.Logging.Sample.Application.Common.Logging;
using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Options;
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
        builder.Services.AddScoped<ILogContext, MelLogContext>();
        builder.Services.AddSingleton<ILogRecorder, InMemoryLogRecorder>();
        builder.Services.AddScoped(typeof(IAppLogger<>), typeof(CompositeAppLogger<>));

        builder.Services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenRequestPreProcessor(typeof(LoggingBehaviour<>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
        });
    }
}
