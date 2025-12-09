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

    /// <summary>
    /// Verifies trace identifiers prefer the explicit value when provided.
    /// </summary>
    [Fact]
    public void Failure_UsesProvidedTraceId()
    {
        var result = Result.Failure(new Error("err", "message", "inner"), "outer");

        Assert.True(result.IsFailure);
        Assert.Equal("outer", result.TraceId);
        Assert.All(result.Errors, e => Assert.Equal("outer", e.TraceId));
    }

    /// <summary>
    /// Verifies combining an empty set of results yields a success.
    /// </summary>
    [Fact]
    public void Combine_WithNoResults_ReturnsSuccess()
    {
        var combined = Result.Combine(Array.Empty<Result>());

        Assert.True(combined.IsSuccess);
    }

    /// <summary>
    /// Verifies accessing Value on a failed result throws.
    /// </summary>
    [Fact]
    public void Value_ThrowsOnFailure()
    {
        var failure = Result.Failure<int>(new Error("err", "message"));

        Assert.Throws<InvalidOperationException>(() => _ = failure.Value);
    }

    /// <summary>
    /// Verifies tap is skipped for failures and preserves the original instance.
    /// </summary>
    [Fact]
    public void Tap_IsSkippedOnFailure()
    {
        var failure = Result.Failure<int>(new Error("err", "message"));
        var called = false;

        var tapped = failure.Tap(_ => called = true);

        Assert.False(called);
        Assert.Same(failure, tapped);
    }

    /// <summary>
    /// Verifies match dispatches to the failure delegate when the result is unsuccessful.
    /// </summary>
    [Fact]
    public void Match_InvokesFailureDelegate()
    {
        var failure = Result.Failure<int>(new Error("err", "message"));

        var value = failure.Match(_ => "success", errors => errors[0].Code);

        Assert.Equal("err", value);
    }

    /// <summary>
    /// Verifies the Ensure extension returns a failure while preserving trace identifiers.
    /// </summary>
    [Fact]
    public void EnsureExtension_UsesResultTraceId()
    {
        var success = Result.Success("value", "trace-ensure");

        var ensured = success.Ensure(() => false, new Error("guard", "failed"));

        Assert.True(ensured.IsFailure);
        Assert.Equal("trace-ensure", ensured.TraceId);
    }

    /// <summary>
    /// Verifies Recover converts a failure into a success using the fallback.
    /// </summary>
    [Fact]
    public void Recover_ReturnsFallbackOnFailure()
    {
        var failure = Result.Failure<int>(new Error("err", "message", "trace-recover"));

        var recovered = failure.Recover(errors => errors.Count);

        Assert.True(recovered.IsSuccess);
        Assert.Equal(1, recovered.Value);
        Assert.Equal("trace-recover", recovered.TraceId);
    }

    /// <summary>
    /// Verifies ToResult wraps values into successful results.
    /// </summary>
    [Fact]
    public void ToResult_WrapsValue()
    {
        var wrapped = 5.ToResult("trace-value");

        Assert.True(wrapped.IsSuccess);
        Assert.Equal(5, wrapped.Value);
        Assert.Equal("trace-value", wrapped.TraceId);
    }

    /// <summary>
    /// Verifies generic failures can be created for runtime types.
    /// </summary>
    [Fact]
    public void ResultFailureFactory_CreatesTypedFailure()
    {
        var errors = new[] { new Error("err", "message", "trace-runtime") };

        var failure = ResultFailureFactory.CreateGenericFailure(typeof(string), errors, "trace-runtime") as Result<string>;

        Assert.NotNull(failure);
        Assert.True(failure!.IsFailure);
        Assert.Equal("trace-runtime", failure.TraceId);
        Assert.Equal("err", failure.Errors[0].Code);
    }

    /// <summary>
    /// Verifies error metadata helpers add entries without mutating the original instance.
    /// </summary>
    [Fact]
    public void Error_WithMetadata_AddsEntry()
    {
        var error = new Error("code", "message");

        var enriched = error.WithMetadata("Key", "Value");

        Assert.False(error.HasMetadata);
        Assert.True(enriched.HasMetadata);
        Assert.Equal("Value", enriched.Metadata["Key"]);
    }
}
