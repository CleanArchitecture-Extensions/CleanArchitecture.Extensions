using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Results;
using CleanArchitecture.Extensions.Validation.Options;
using CleanArchitecture.Extensions.Validation.Rules;
using CleanArchitecture.Extensions.Validation.Validators;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Extensions.Validation.Tests;

/// <summary>
/// Tests covering DI helpers for the validation package.
/// </summary>
public class ValidationDependencyInjectionTests
{
    [Fact]
    public async Task AddCleanArchitectureValidationPipeline_RegistersBehavior()
    {
        var services = new ServiceCollection();
        services.AddCleanArchitectureValidation(options => options.Strategy = ValidationStrategy.ReturnResult);
        services.AddLogging();
        services.AddSingleton(typeof(IAppLogger<>), typeof(InMemoryAppLogger<>));
        services.AddTransient<IValidator<Ping>, PingValidator>();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<ValidationDependencyInjectionTests>();
            cfg.AddCleanArchitectureValidationPipeline();
        });

        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new Ping(string.Empty));

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "VAL.EMPTY");
    }

    [Fact]
    public void AddCleanArchitectureValidation_ConfiguresOptions()
    {
        var services = new ServiceCollection();
        services.AddCleanArchitectureValidation(options =>
        {
            options.MaxFailures = 10;
            options.TraceId = "trace-di";
        });
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ValidationOptions>>().Value;

        Assert.Equal(10, options.MaxFailures);
        Assert.Equal("trace-di", options.TraceId);
    }

    private sealed record Ping(string Name) : IRequest<Result>;

    private sealed class PingValidator : AbstractValidatorBase<Ping>
    {
        public PingValidator()
        {
            RuleFor(x => x.Name).NotEmptyTrimmed();
        }
    }
}
