using System.Linq;
using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Results;
using CleanArchitecture.Extensions.Exceptions.BaseTypes;
using CleanArchitecture.Extensions.Exceptions.Behaviors;
using CleanArchitecture.Extensions.Exceptions.Catalog;
using CleanArchitecture.Extensions.Exceptions.Options;
using CleanArchitecture.Extensions.Exceptions.Redaction;

namespace CleanArchitecture.Extensions.Exceptions.Tests;

/// <summary>
/// Tests for <see cref="ExceptionWrappingBehavior{TRequest, TResponse}"/>.
/// </summary>
public class ExceptionWrappingBehaviorTests
{
    [Fact]
    public async Task Handle_MapsExceptionToResult_WhenResponseIsResult()
    {
        var logger = new InMemoryAppLogger<TestRequest>();
        var logContext = new InMemoryLogContext { CorrelationId = "cid-test" };
        var behavior = new ExceptionWrappingBehavior<TestRequest, Result>(
            new ExceptionCatalog(),
            Microsoft.Extensions.Options.Options.Create(ExceptionHandlingOptions.Default),
            new ExceptionRedactor(),
            logger,
            logContext);

        var result = await behavior.Handle(
            new TestRequest(),
            _ => throw new NotFoundException("Order", 1),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        var error = result.Errors.Single();
        Assert.Equal(ExceptionCodes.NotFound, error.Code);
        Assert.Equal("cid-test", result.TraceId);
        Assert.Equal("cid-test", error.TraceId);
    }

    [Fact]
    public async Task Handle_Rethrows_WhenTypeIsConfiguredToBypassWrapping()
    {
        var behavior = new ExceptionWrappingBehavior<TestRequest, Result>(
            new ExceptionCatalog(),
            Microsoft.Extensions.Options.Options.Create(ExceptionHandlingOptions.Default));

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            behavior.Handle(new TestRequest(), _ => throw new OperationCanceledException(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_LogsExceptionWithCorrelationMetadata()
    {
        var logContext = new InMemoryLogContext { CorrelationId = "cid-log" };
        var logger = new InMemoryAppLogger<TestRequest>(logContext);
        var behavior = new ExceptionWrappingBehavior<TestRequest, Result>(
            new ExceptionCatalog(),
            Microsoft.Extensions.Options.Options.Create(ExceptionHandlingOptions.Default),
            new ExceptionRedactor(),
            logger,
            logContext);

        _ = await behavior.Handle(
            new TestRequest(),
            _ => throw new ConflictException(),
            CancellationToken.None);

        Assert.Contains(logger.Entries, entry => entry.CorrelationId == "cid-log" && entry.Level == LogLevel.Error);
    }

    [Fact]
    public async Task Handle_MapsExceptionToGenericResult()
    {
        var behavior = new ExceptionWrappingBehavior<TestRequest, Result<string>>(
            new ExceptionCatalog(),
            Microsoft.Extensions.Options.Options.Create(ExceptionHandlingOptions.Default));

        var result = await behavior.Handle(
            new TestRequest(),
            _ => throw new ForbiddenException(),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ExceptionCodes.Forbidden, result.Errors.Single().Code);
    }

    [Fact]
    public async Task Handle_UsesCatalogMessage_WhenDetailsAreHidden()
    {
        var options = new ExceptionHandlingOptions { IncludeExceptionDetails = false };
        var behavior = new ExceptionWrappingBehavior<TestRequest, Result>(
            new ExceptionCatalog(),
            Microsoft.Extensions.Options.Options.Create(options));

        const string message = "Order 9 was not found for tenant foo.";
        var result = await behavior.Handle(
            new TestRequest(),
            _ => throw new NotFoundException(message),
            CancellationToken.None);

        var error = result.Errors.Single();
        Assert.Equal("The specified resource was not found.", error.Message);
    }

    [Fact]
    public async Task Handle_FlowsApplicationExceptionMessage_WhenDetailsAreIncluded()
    {
        var options = new ExceptionHandlingOptions { IncludeExceptionDetails = true };
        var behavior = new ExceptionWrappingBehavior<TestRequest, Result>(
            new ExceptionCatalog(),
            Microsoft.Extensions.Options.Options.Create(options));

        const string message = "Order 9 was not found for tenant foo.";
        var result = await behavior.Handle(
            new TestRequest(),
            _ => throw new NotFoundException(message),
            CancellationToken.None);

        var error = result.Errors.Single();
        Assert.Equal(message, error.Message);
    }

    private sealed record TestRequest;
}
