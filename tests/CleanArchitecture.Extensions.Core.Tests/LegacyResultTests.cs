using CleanArchitecture.Extensions.Core.Results;

namespace CleanArchitecture.Extensions.Core.Tests;

/// <summary>
/// Tests covering compatibility adapters for the template-style Result shape.
/// </summary>
public class LegacyResultTests
{
    [Fact]
    public void FromResult_MapsSuccess()
    {
        var core = Result.Success();

        var legacy = LegacyResult.FromResult(core);

        Assert.True(legacy.Succeeded);
        Assert.Empty(legacy.Errors);
    }

    [Fact]
    public void FromResult_MapsFailureMessages()
    {
        var core = Result.Failure(new Error("err", "message"));

        var legacy = LegacyResult.FromResult(core);

        Assert.False(legacy.Succeeded);
        Assert.Contains("message", legacy.Errors);
    }

    [Fact]
    public void ToResult_FromLegacySuccess_CreatesSuccess()
    {
        var legacy = LegacyResult.Success();

        var core = legacy.ToResult("trace-1");

        Assert.True(core.IsSuccess);
        Assert.Equal("trace-1", core.TraceId);
    }

    [Fact]
    public void ToResult_FromLegacyFailure_CreatesErrors()
    {
        var legacy = LegacyResult.Failure("oops");

        var core = legacy.ToResult("trace-2", "legacy.code");

        Assert.True(core.IsFailure);
        Assert.Equal("trace-2", core.TraceId);
        Assert.Contains(core.Errors, e => e.Code == "legacy.code" && e.Message == "oops");
    }

    [Fact]
    public void LegacyResultT_FromResult_MapsValue()
    {
        var core = Result.Success(5, "trace-3");

        var legacy = LegacyResult<int>.FromResult(core);

        Assert.True(legacy.Succeeded);
        Assert.Equal(5, legacy.Value);
    }

    [Fact]
    public void LegacyResultT_ToResult_MapsErrors()
    {
        var legacy = LegacyResult<int>.Failure(new[] { "first", "second" });

        var core = legacy.ToResult("trace-4");

        Assert.True(core.IsFailure);
        Assert.Equal("trace-4", core.TraceId);
        Assert.Equal(2, core.Errors.Count);
    }
}
