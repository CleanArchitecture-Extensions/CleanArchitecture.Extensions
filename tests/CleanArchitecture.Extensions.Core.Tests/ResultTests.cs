using CleanArchitecture.Extensions.Core.Results;

namespace CleanArchitecture.Extensions.Core.Tests;

/// <summary>
/// Tests covering the <see cref="Result"/> and <see cref="Result{T}"/> combinators.
/// </summary>
public class ResultTests
{
    /// <summary>
    /// Verifies that mapping a successful result transforms the value.
    /// </summary>
    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        var result = Result.Success(2);

        var mapped = result.Map(value => value * 2);

        Assert.True(mapped.IsSuccess);
        Assert.Equal(4, mapped.Value);
    }

    /// <summary>
    /// Verifies that binding a failed result short-circuits the computation.
    /// </summary>
    [Fact]
    public void Bind_OnFailure_ShortCircuits()
    {
        var failure = Result.Failure<int>(new Error("err", "problem"));

        var bound = failure.Bind(value => Result.Success(value * 10));

        Assert.True(bound.IsFailure);
        Assert.Equal("err", bound.Errors[0].Code);
    }

    /// <summary>
    /// Verifies that combining results aggregates error collections.
    /// </summary>
    [Fact]
    public void Combine_AggregatesErrors()
    {
        var first = Result.Success();
        var second = Result.Failure(new Error("err-1", "first"));
        var third = Result.Failure(new Error("err-2", "second"));

        var combined = Result.Combine(first, second, third);

        Assert.True(combined.IsFailure);
        Assert.Equal(2, combined.Errors.Count);
    }

    /// <summary>
    /// Verifies that Ensure returns a failure when predicate does not hold.
    /// </summary>
    [Fact]
    public void Ensure_FailsWhenPredicateIsFalse()
    {
        var result = Result.Success(5).Ensure(value => value > 10, new Error("too-small", "value is too small"));

        Assert.True(result.IsFailure);
        Assert.Equal("too-small", result.Errors[0].Code);
    }
}
