using CleanArchitecture.Extensions.Core.Guards;
using CleanArchitecture.Extensions.Core.Options;
using CleanArchitecture.Extensions.Core.Results;

namespace CleanArchitecture.Extensions.Core.Tests;

/// <summary>
/// Tests covering guard clause strategies and behaviors.
/// </summary>
public class GuardTests
{
    /// <summary>
    /// Ensures the default guard strategy returns a failure result for null inputs.
    /// </summary>
    [Fact]
    public void AgainstNull_DefaultStrategy_ReturnsFailure()
    {
        var result = Guard.AgainstNull<string>(null, "name");

        Assert.True(result.IsFailure);
        Assert.Equal("guard.null", result.Errors[0].Code);
    }

    /// <summary>
    /// Ensures the throw strategy raises exceptions on guard failures.
    /// </summary>
    [Fact]
    public void AgainstNull_WithThrowStrategy_Throws()
    {
        var options = new GuardOptions { Strategy = GuardStrategy.Throw };

        Assert.Throws<ArgumentException>(() => Guard.AgainstNull<string>(null, "name", options));
    }

    /// <summary>
    /// Ensures errors are captured in the sink when using the accumulate strategy.
    /// </summary>
    [Fact]
    public void AccumulateStrategy_CapturesErrors()
    {
        var errors = new List<Error>();
        var options = new GuardOptions
        {
            Strategy = GuardStrategy.Accumulate,
            ErrorSink = errors
        };

        var result = Guard.AgainstTooShort("a", 2, "name", options);

        Assert.True(result.IsFailure);
        Assert.Single(errors);
    }

    /// <summary>
    /// Ensures in-range values return a success result.
    /// </summary>
    [Fact]
    public void AgainstOutOfRange_ReturnsSuccessWhenWithinBounds()
    {
        var result = Guard.AgainstOutOfRange(5, 1, 10, "value");

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value);
    }

    /// <summary>
    /// Ensures whitespace-only strings are rejected.
    /// </summary>
    [Fact]
    public void AgainstNullOrWhiteSpace_ReturnsFailure()
    {
        var result = Guard.AgainstNullOrWhiteSpace("  ", "value");

        Assert.True(result.IsFailure);
        Assert.Equal("guard.empty", result.Errors[0].Code);
    }

    /// <summary>
    /// Ensures undefined enum values are rejected with the expected code.
    /// </summary>
    [Fact]
    public void AgainstUndefinedEnum_ReturnsFailure()
    {
        var result = Guard.AgainstUndefinedEnum((TestEnum)99, "enum");

        Assert.True(result.IsFailure);
        Assert.Equal("guard.enum", result.Errors[0].Code);
    }

    /// <summary>
    /// Ensures defined enum values pass validation.
    /// </summary>
    [Fact]
    public void AgainstUndefinedEnum_ReturnsSuccessForDefinedValue()
    {
        var result = Guard.AgainstUndefinedEnum(TestEnum.One, "enum");

        Assert.True(result.IsSuccess);
        Assert.Equal(TestEnum.One, result.Value);
    }

    /// <summary>
    /// Ensures string length checks catch overly long inputs.
    /// </summary>
    [Fact]
    public void AgainstTooLong_ReturnsFailure()
    {
        var result = Guard.AgainstTooLong("abcdef", 3, "value");

        Assert.True(result.IsFailure);
        Assert.Equal("guard.length", result.Errors[0].Code);
    }

    /// <summary>
    /// Ensures guard Ensure returns success when the condition passes.
    /// </summary>
    [Fact]
    public void Ensure_SucceedsWhenConditionTrue()
    {
        var result = Guard.Ensure(true, "ok", "all good");

        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Ensures guard errors carry through trace identifiers.
    /// </summary>
    [Fact]
    public void Ensure_UsesTraceIdFromOptions()
    {
        var options = new GuardOptions { TraceId = "trace-guard" };

        var result = Guard.Ensure(false, "code", "message", options);

        Assert.True(result.IsFailure);
        Assert.Equal("trace-guard", result.Errors[0].TraceId);
    }

    /// <summary>
    /// Ensures guard options can be materialized from shared core options.
    /// </summary>
    [Fact]
    public void GuardOptions_FromOptions_CopiesSharedSettings()
    {
        var sink = new List<Error>();
        var coreOptions = new CoreExtensionsOptions
        {
            GuardStrategy = GuardStrategy.Accumulate,
            TraceId = "trace-core"
        };

        var guardOptions = GuardOptions.FromOptions(coreOptions, sink);

        Assert.Equal(GuardStrategy.Accumulate, guardOptions.Strategy);
        Assert.Equal("trace-core", guardOptions.TraceId);
        Assert.Same(sink, guardOptions.ErrorSink);
    }

    private enum TestEnum
    {
        One = 1,
        Two = 2
    }
}
