using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Results;
using CleanArchitecture.Extensions.Exceptions.BaseTypes;
using CleanArchitecture.Extensions.Exceptions.Catalog;
using CleanArchitecture.Extensions.Exceptions.Options;
using CleanArchitecture.Extensions.Exceptions.Redaction;
using CleanArchitecture.Extensions.Exceptions.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Extensions.Exceptions.Tests;

/// <summary>
/// Tests covering DI registration helpers for the exceptions package.
/// </summary>
public class DependencyInjectionTests
{
    [Fact]
    public async Task AddCleanArchitectureExceptionsPipeline_RegistersBehaviorAndWraps()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCleanArchitectureExceptions(options => options.TraceId = "trace-di");
        services.AddScoped(typeof(IAppLogger<>), typeof(InMemoryAppLogger<>));
        services.AddScoped<ILogContext, InMemoryLogContext>();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<DependencyInjectionTests>();
            cfg.AddCleanArchitectureExceptionsPipeline();
        });
        services.AddTransient<IRequestHandler<Ping, Result>>(sp => new PingHandler());

        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = false });
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new Ping());

        Assert.True(result.IsFailure);
        Assert.Equal("trace-di", result.TraceId);
        Assert.Contains(result.Errors, e => e.Code == ExceptionCodes.Unknown);
    }

    [Fact]
    public void AddCleanArchitectureExceptions_RegistersCatalogAndRedactor()
    {
        var services = new ServiceCollection();
        services.AddCleanArchitectureExceptions(options => options.IncludeExceptionDetails = true, configureCatalog: catalog =>
        {
            catalog.Descriptors.Add(new ExceptionDescriptor(typeof(InvalidOperationException), "custom", "custom message"));
        });

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IExceptionCatalog>());
        Assert.NotNull(provider.GetRequiredService<ExceptionRedactor>());
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ExceptionHandlingOptions>>().Value;
        Assert.True(options.IncludeExceptionDetails);
    }

    private sealed record Ping : IRequest<Result>;

    private sealed class PingHandler : IRequestHandler<Ping, Result>
    {
        public Task<Result> Handle(Ping request, CancellationToken cancellationToken)
        {
            throw new Exception("boom");
        }
    }
}
