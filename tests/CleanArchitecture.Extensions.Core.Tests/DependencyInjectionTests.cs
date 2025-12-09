using CleanArchitecture.Extensions.Core;
using CleanArchitecture.Extensions.Core.Behaviors;
using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Options;
using CleanArchitecture.Extensions.Core.Time;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

    [Fact]
    public void AddCleanArchitectureCore_RegistersDefaultsAndAppliesConfiguration()
    {
        var services = new ServiceCollection();

        services.AddCleanArchitectureCore(options => options.TraceId = "trace-di");
        services.AddScoped(typeof(IAppLogger<>), typeof(InMemoryAppLogger<>));
        using var provider = services.BuildServiceProvider();

        var clock = provider.GetRequiredService<IClock>();
        var logContext = provider.GetRequiredService<ILogContext>();
        var options = provider.GetRequiredService<IOptions<CoreExtensionsOptions>>().Value;
        var tracker = provider.GetRequiredService<DomainEvents.DomainEventTracker>();

        Assert.IsType<SystemClock>(clock);
        Assert.IsType<InMemoryLogContext>(logContext);
        Assert.Equal("trace-di", options.TraceId);
        Assert.NotNull(tracker);
    }

    [Fact]
    public void AddCleanArchitectureCorePipeline_HonorsOptionFlags()
    {
        var services = new ServiceCollection();

        services.AddCleanArchitectureCore();
        services.AddScoped(typeof(IAppLogger<>), typeof(InMemoryAppLogger<>));
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<DependencyInjectionTests>();
            cfg.AddCleanArchitectureCorePipeline(options =>
            {
                options.UsePerformanceBehavior = false;
                options.UseLoggingPreProcessor = false;
            });
        });

        using var provider = services.BuildServiceProvider();
        var preProcessors = provider.GetServices<IRequestPreProcessor<Echo>>().ToList();
        var behaviors = provider.GetServices<IPipelineBehavior<Echo, string>>().ToList();

        Assert.Empty(preProcessors);
        Assert.DoesNotContain(behaviors, behavior => behavior is PerformanceBehavior<Echo, string>);
        Assert.Contains(behaviors, behavior => behavior is CorrelationBehavior<Echo, string>);
        Assert.Contains(behaviors, behavior => behavior is LoggingBehavior<Echo, string>);
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
