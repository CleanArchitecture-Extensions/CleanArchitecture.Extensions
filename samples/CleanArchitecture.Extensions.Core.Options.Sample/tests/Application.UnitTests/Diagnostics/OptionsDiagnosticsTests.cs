using CleanArchitecture.Extensions.Core.Guards;
using CleanArchitecture.Extensions.Core.Options;
using CleanArchitecture.Extensions.Core.Options.Sample.Application.Diagnostics.Commands.EvaluateName;
using CleanArchitecture.Extensions.Core.Options.Sample.Application.Diagnostics.Queries.GetCoreOptions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Extensions.Core.Options.Sample.Application.UnitTests.Diagnostics;

public class OptionsDiagnosticsTests
{
    [Test]
    public async Task GetCoreOptions_ReturnsConfiguredValues()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new CoreExtensionsOptions
        {
            CorrelationHeaderName = "X-Test",
            GuardStrategy = GuardStrategy.ReturnFailure,
            EnablePerformanceLogging = true,
            PerformanceWarningThreshold = TimeSpan.FromMilliseconds(123)
        });

        var handler = new GetCoreOptionsQueryHandler(options);

        var dto = await handler.Handle(new GetCoreOptionsQuery(), CancellationToken.None);

        dto.CorrelationHeaderName.ShouldBe("X-Test");
        dto.GuardStrategy.ShouldBe(GuardStrategy.ReturnFailure);
        dto.EnablePerformanceLogging.ShouldBeTrue();
        dto.PerformanceWarningThreshold.ShouldBe(TimeSpan.FromMilliseconds(123));
    }

    [Test]
    public async Task EvaluateName_RespectsGuardStrategy_ReturnFailure()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new CoreExtensionsOptions
        {
            GuardStrategy = GuardStrategy.ReturnFailure
        });
        var handler = new EvaluateNameCommandHandler(options);

        var result = await handler.Handle(new EvaluateNameCommand("ok"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();

        var failure = await handler.Handle(new EvaluateNameCommand(""), CancellationToken.None);
        failure.IsFailure.ShouldBeTrue();
    }

    [Test]
    public void EvaluateName_Throws_WhenStrategyThrow()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new CoreExtensionsOptions
        {
            GuardStrategy = GuardStrategy.Throw
        });
        var handler = new EvaluateNameCommandHandler(options);

        Should.ThrowAsync<Exception>(() => handler.Handle(new EvaluateNameCommand(""), CancellationToken.None));
    }
}
