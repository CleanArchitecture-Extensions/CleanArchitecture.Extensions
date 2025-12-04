using CleanArchitecture.Extensions.Core.Behaviors;
using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Options;
using CleanArchitecture.Extensions.Core.Time;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Extensions.Core.Tests;

/// <summary>
/// Tests covering DI registration paths to ensure pipelines resolve.
/// </summary>
public class DependencyInjectionTests
{
    [Fact]
    public async Task ServiceCollection_ResolvesPipelineAndHandler()
    {
        var services = new ServiceCollection();

        services.Configure<CoreExtensionsOptions>(_ => { });
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<ILogContext, InMemoryLogContext>();
        services.AddScoped(typeof(IAppLogger<>), typeof(InMemoryAppLogger<>));
        services.AddTransient(typeof(IRequestPreProcessor<>), typeof(LoggingPreProcessor<>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CorrelationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        services.AddTransient(typeof(CorrelationBehavior<,>), typeof(CorrelationBehavior<,>));
        services.AddTransient(typeof(LoggingBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(PerformanceBehavior<,>), typeof(PerformanceBehavior<,>));
        services.AddTransient<IRequestHandler<Echo, string>, EchoHandler>();
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        await using var scope = provider.CreateAsyncScope();
        var correlation = scope.ServiceProvider.GetRequiredService<CorrelationBehavior<Echo, string>>();
        var logging = scope.ServiceProvider.GetRequiredService<LoggingBehavior<Echo, string>>();
        var performance = scope.ServiceProvider.GetRequiredService<PerformanceBehavior<Echo, string>>();
        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<Echo, string>>();

        var request = new Echo("hello");
        var cancellationToken = CancellationToken.None;
        RequestHandlerDelegate<string> final = ct => handler.Handle(request, ct);
        RequestHandlerDelegate<string> perfLayer = ct => performance.Handle(request, final, ct);
        RequestHandlerDelegate<string> loggingLayer = ct => logging.Handle(request, perfLayer, ct);

        var result = await correlation.Handle(request, loggingLayer, cancellationToken);
        var context = scope.ServiceProvider.GetRequiredService<ILogContext>();

        Assert.Equal("hello", result);
        Assert.False(string.IsNullOrWhiteSpace(context.CorrelationId));
    }

    private sealed record Echo(string Message) : IRequest<string>;

    private sealed class EchoHandler : IRequestHandler<Echo, string>
    {
        private readonly ILogContext _context;

        public EchoHandler(ILogContext context)
        {
            _context = context;
        }

        public Task<string> Handle(Echo request, CancellationToken cancellationToken)
        {
            return Task.FromResult(request.Message);
        }
    }
}
