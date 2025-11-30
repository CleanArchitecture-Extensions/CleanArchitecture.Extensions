using CleanArchitecture.Extensions.Core.Guards;
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
}
