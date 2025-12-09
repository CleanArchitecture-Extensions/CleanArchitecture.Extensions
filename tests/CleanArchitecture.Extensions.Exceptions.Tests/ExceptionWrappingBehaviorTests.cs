using System.Linq;
using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Options;
using CleanArchitecture.Extensions.Core.Results;
using CleanArchitecture.Extensions.Exceptions.BaseTypes;
using CleanArchitecture.Extensions.Exceptions.Behaviors;
using CleanArchitecture.Extensions.Exceptions.Catalog;
using CleanArchitecture.Extensions.Exceptions.Options;
using CleanArchitecture.Extensions.Exceptions.Redaction;
using TemplateResult = CleanArchitecture.Extensions.Exceptions.Tests.ExceptionWrappingBehaviorTests.TemplateResultContainer.Result;

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
    public async Task Handle_RethrowsNormalizedCancellation_WhenAggregateException()
    {
        var behavior = new ExceptionWrappingBehavior<TestRequest, Result>(
            new ExceptionCatalog(),
            Microsoft.Extensions.Options.Options.Create(ExceptionHandlingOptions.Default));

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            behavior.Handle(new TestRequest(), _ => throw new AggregateException(new OperationCanceledException()), CancellationToken.None));
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

    [Fact]
    public async Task Handle_AppliesStatusOverrideAndTraceResolutionOrder()
    {
        var options = new ExceptionHandlingOptions
        {
            StatusCodeOverrides = { [ExceptionCodes.NotFound] = System.Net.HttpStatusCode.Gone },
            TraceId = "options-trace"
        };
        var logContext = new InMemoryLogContext { CorrelationId = "context-trace" };
        var coreOptions = Microsoft.Extensions.Options.Options.Create(new CoreExtensionsOptions { TraceId = "core-trace" });
        var behavior = new ExceptionWrappingBehavior<TestRequest, Result>(
            new ExceptionCatalog(),
            Microsoft.Extensions.Options.Options.Create(options),
            redactor: new ExceptionRedactor(),
            logContext: logContext,
            coreOptions: coreOptions);

        var result = await behavior.Handle(new TestRequest(), _ => throw new NotFoundException(), CancellationToken.None);

        var error = result.Errors.Single();
        Assert.Equal("options-trace", result.TraceId);
        Assert.Equal(((int)System.Net.HttpStatusCode.Gone).ToString(), error.Metadata["status"]);
    }

    [Fact]
    public async Task Handle_UsesTemplateResultFailureFactory()
    {
        var behavior = new ExceptionWrappingBehavior<TestRequest, TemplateResult>(
            new ExceptionCatalog(),
            Microsoft.Extensions.Options.Options.Create(new ExceptionHandlingOptions { ConvertToResult = true }));

        var result = await behavior.Handle(new TestRequest(), _ => throw new ConflictException("fail"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Contains(ExceptionCodes.Conflict, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Handle_LogsExceptionAndIncludesExceptionObject_WhenStackTraceEnabled()
    {
        var logContext = new InMemoryLogContext { CorrelationId = "cid-log" };
        var logger = new InMemoryAppLogger<TestRequest>(logContext);
        var options = new ExceptionHandlingOptions
        {
            IncludeExceptionDetails = true,
            IncludeStackTrace = true,
            RedactSensitiveData = false
        };
        var behavior = new ExceptionWrappingBehavior<TestRequest, Result>(
            new ExceptionCatalog(),
            Microsoft.Extensions.Options.Options.Create(options),
            new ExceptionRedactor(),
            logger,
            logContext);

        _ = await behavior.Handle(new TestRequest(), _ => throw new DomainException("domain failure"), CancellationToken.None);

        var entry = Assert.Single(logger.Entries);
        Assert.NotNull(entry.Exception);
        Assert.Equal(logContext.CorrelationId, entry.CorrelationId);
    }

    internal static class TemplateResultContainer
    {
        internal sealed class Result
        {
            public bool Succeeded { get; private set; }
            public IReadOnlyList<string> Errors { get; private set; } = Array.Empty<string>();

            public static Result Failure(IEnumerable<string> errors) => new()
            {
                Succeeded = false,
                Errors = errors.ToArray()
            };
        }
    }

    private sealed record TestRequest;
}
