using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Options;
using CleanArchitecture.Extensions.Core.Results;
using CleanArchitecture.Extensions.Validation.Behaviors;
using CleanArchitecture.Extensions.Validation.Exceptions;
using CleanArchitecture.Extensions.Validation.Models;
using CleanArchitecture.Extensions.Validation.Notifications;
using CleanArchitecture.Extensions.Validation.Options;
using CleanArchitecture.Extensions.Validation.Rules;
using CleanArchitecture.Extensions.Validation.Validators;
using FluentValidation;
using MediatR;
using MicrosoftOptions = Microsoft.Extensions.Options.Options;
using ValidationException = CleanArchitecture.Extensions.Validation.Exceptions.ValidationException;

namespace CleanArchitecture.Extensions.Validation.Tests;

public sealed record FakeRequest(string Name, int PageSize, string? Email);

public class ValidationBehaviourTests
{
    [Fact]
    public async Task Handle_NoValidators_CallsNext()
    {
        var behavior = new ValidationBehaviour<FakeRequest, Result>(
            Array.Empty<IValidator<FakeRequest>>(),
            MicrosoftOptions.Create(new ValidationOptions { Strategy = ValidationStrategy.ReturnResult }));
        var nextCalled = false;
        RequestHandlerDelegate<Result> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Success());
        };

        var result = await behavior.Handle(new FakeRequest("ok", 5, "test@example.com"), next, CancellationToken.None);

        Assert.True(nextCalled);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WithFailures_DefaultThrowsValidationException()
    {
        var validator = new FakeRequestValidator();
        var behavior = new ValidationBehaviour<FakeRequest, Result>(new[] { validator }, MicrosoftOptions.Create(ValidationOptions.Default));

        await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(new FakeRequest(string.Empty, 0, "bad-email"), _ => Task.FromResult(Result.Success()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithReturnResultStrategy_ShortCircuitsWithFailureResult()
    {
        var validator = new FakeRequestValidator();
        var options = new ValidationOptions
        {
            Strategy = ValidationStrategy.ReturnResult,
            IncludeAttemptedValue = true,
            TraceId = "trace-123"
        };
        var behavior = new ValidationBehaviour<FakeRequest, Result>(new[] { validator }, MicrosoftOptions.Create(options));
        var nextCalled = false;

        RequestHandlerDelegate<Result> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Success());
        };

        var result = await behavior.Handle(new FakeRequest(string.Empty, 0, "not-an-email"), next, CancellationToken.None);

        Assert.False(nextCalled);
        Assert.True(result.IsFailure);
        Assert.Equal("trace-123", result.TraceId);
        Assert.Equal(3, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Code == "VAL.EMPTY");
        Assert.Contains(result.Errors, e => e.Metadata.ContainsKey("property"));
    }

    [Fact]
    public async Task Handle_RespectsMaxFailures()
    {
        var validator = new MultiFailureValidator();
        var options = MicrosoftOptions.Create(new ValidationOptions { Strategy = ValidationStrategy.ReturnResult, MaxFailures = 1 });
        var behavior = new ValidationBehaviour<FakeRequest, Result>(new[] { validator }, options);

        var result = await behavior.Handle(new FakeRequest(string.Empty, 1, null), _ => Task.FromResult(Result.Success()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task Handle_NotifyPublishesAndReturnsResult()
    {
        var validator = new FakeRequestValidator();
        var publisher = new RecordingPublisher();
        var options = MicrosoftOptions.Create(new ValidationOptions { Strategy = ValidationStrategy.Notify, NotifyBehavior = ValidationNotifyBehavior.ReturnResult });
        var behavior = new ValidationBehaviour<FakeRequest, Result>(new[] { validator }, options, publisher);

        var result = await behavior.Handle(new FakeRequest(string.Empty, 0, "bad-email"), _ => Task.FromResult(Result.Success()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(1, publisher.PublishCount);
        Assert.NotNull(publisher.LastErrors);
        Assert.NotEmpty(publisher.LastErrors!);
    }

    [Fact]
    public async Task Handle_UsesValidationOptionsTraceIdOverContextAndCoreOptions()
    {
        var validator = new FakeRequestValidator();
        var logContext = new InMemoryLogContext { CorrelationId = "corr-override" };
        var options = new ValidationOptions
        {
            Strategy = ValidationStrategy.ReturnResult,
            TraceId = "options-trace"
        };
        var coreOptions = MicrosoftOptions.Create(new CoreExtensionsOptions { TraceId = "core-trace" });
        var behavior = new ValidationBehaviour<FakeRequest, Result>(
            new[] { validator },
            MicrosoftOptions.Create(options),
            logContext: logContext,
            coreOptions: coreOptions);

        var result = await behavior.Handle(new FakeRequest(string.Empty, 0, "bad-email"), _ => Task.FromResult(Result.Success()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("options-trace", result.TraceId);
        Assert.All(result.Errors, error => Assert.Equal("options-trace", error.TraceId));
    }

    [Fact]
    public async Task Handle_FallsBackToCorrelationIdWhenTraceIdMissing()
    {
        var validator = new FakeRequestValidator();
        var logContext = new InMemoryLogContext { CorrelationId = "corr-fallback" };
        var behavior = new ValidationBehaviour<FakeRequest, Result>(
            new[] { validator },
            MicrosoftOptions.Create(new ValidationOptions { Strategy = ValidationStrategy.ReturnResult }),
            logContext: logContext,
            coreOptions: MicrosoftOptions.Create(new CoreExtensionsOptions { TraceId = "core-trace" }));

        var result = await behavior.Handle(new FakeRequest(string.Empty, 0, "bad-email"), _ => Task.FromResult(Result.Success()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("corr-fallback", result.TraceId);
        Assert.All(result.Errors, error => Assert.Equal("corr-fallback", error.TraceId));
    }

    [Fact]
    public async Task Handle_FallsBackToCoreOptionsTraceIdWhenContextUnavailable()
    {
        var validator = new FakeRequestValidator();
        var coreOptions = MicrosoftOptions.Create(new CoreExtensionsOptions { TraceId = "core-only-trace" });
        var behavior = new ValidationBehaviour<FakeRequest, Result>(
            new[] { validator },
            MicrosoftOptions.Create(new ValidationOptions { Strategy = ValidationStrategy.ReturnResult }),
            logContext: null,
            coreOptions: coreOptions);

        var result = await behavior.Handle(new FakeRequest(string.Empty, 0, "bad-email"), _ => Task.FromResult(Result.Success()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("core-only-trace", result.TraceId);
        Assert.All(result.Errors, error => Assert.Equal("core-only-trace", error.TraceId));
    }

    [Fact]
    public async Task LogValidationFailures_WhenEnabled_LogsWithCorrelationAndTrace()
    {
        var logContext = new InMemoryLogContext { CorrelationId = "corr-log" };
        var logger = new InMemoryAppLogger<FakeRequest>(logContext);
        var options = new ValidationOptions
        {
            Strategy = ValidationStrategy.ReturnResult,
            TraceId = "trace-log",
            LogValidationFailures = true
        };
        var behavior = new ValidationBehaviour<FakeRequest, Result>(
            new[] { new FakeRequestValidator() },
            MicrosoftOptions.Create(options),
            logContext: logContext,
            logger: logger);

        var result = await behavior.Handle(new FakeRequest(string.Empty, 0, "bad-email"), _ => Task.FromResult(Result.Success()), CancellationToken.None);

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Equal("Validation failed for FakeRequest", entry.Message);
        Assert.Equal("corr-log", entry.CorrelationId);

        var properties = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(entry.Properties);
        Assert.Equal(typeof(FakeRequest).FullName ?? typeof(FakeRequest).Name, properties["RequestType"]);
        Assert.Equal("corr-log", properties["CorrelationId"]);
        Assert.Equal("trace-log", properties["TraceId"]);
        Assert.Equal(result.Errors.Count, Assert.IsType<int>(properties["FailureCount"]));
        Assert.Equal(result.Errors.Count, Assert.IsType<int>(properties["TotalFailureCount"]));
        Assert.False(Assert.IsType<bool>(properties["Truncated"]));
        Assert.NotNull(properties["Errors"]);
    }

    [Fact]
    public async Task LogValidationFailures_WhenDisabled_SkipsLogging()
    {
        var logContext = new InMemoryLogContext { CorrelationId = "corr-log" };
        var logger = new InMemoryAppLogger<FakeRequest>(logContext);
        var options = new ValidationOptions
        {
            Strategy = ValidationStrategy.ReturnResult,
            LogValidationFailures = false
        };
        var behavior = new ValidationBehaviour<FakeRequest, Result>(
            new[] { new FakeRequestValidator() },
            MicrosoftOptions.Create(options),
            logContext: logContext,
            logger: logger);

        var result = await behavior.Handle(new FakeRequest(string.Empty, 0, "bad-email"), _ => Task.FromResult(Result.Success()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Empty(logger.Entries);
    }

    [Fact]
    public async Task LogValidationFailures_UsesSeverityMappingForHighestLevel()
    {
        var logContext = new InMemoryLogContext();
        var logger = new InMemoryAppLogger<FakeRequest>(logContext);
        var options = new ValidationOptions
        {
            Strategy = ValidationStrategy.ReturnResult,
            SeverityLogLevels = new Dictionary<Severity, LogLevel>
            {
                [Severity.Warning] = LogLevel.Error,
                [Severity.Info] = LogLevel.Debug
            },
            DefaultLogLevel = LogLevel.Trace
        };
        var behavior = new ValidationBehaviour<FakeRequest, Result>(
            new[] { new MixedSeverityValidator() },
            MicrosoftOptions.Create(options),
            logContext: logContext,
            logger: logger);

        await behavior.Handle(new FakeRequest(string.Empty, 0, null), _ => Task.FromResult(Result.Success()), CancellationToken.None);

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
    }

    [Fact]
    public async Task LogValidationFailures_UsesDefaultLogLevelWhenSeverityNotMapped()
    {
        var logContext = new InMemoryLogContext();
        var logger = new InMemoryAppLogger<FakeRequest>(logContext);
        var options = new ValidationOptions
        {
            Strategy = ValidationStrategy.ReturnResult,
            DefaultLogLevel = LogLevel.Debug,
            SeverityLogLevels = new Dictionary<Severity, LogLevel>
            {
                [Severity.Error] = LogLevel.Critical
            }
        };
        var behavior = new ValidationBehaviour<FakeRequest, Result>(
            new[] { new InfoSeverityValidator() },
            MicrosoftOptions.Create(options),
            logContext: logContext,
            logger: logger);

        await behavior.Handle(new FakeRequest(string.Empty, 1, null), _ => Task.FromResult(Result.Success()), CancellationToken.None);

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Debug, entry.Level);
    }
}

internal sealed class FakeRequestValidator : AbstractValidatorBase<FakeRequest>
{
    public FakeRequestValidator()
    {
        RuleFor(request => request.Name).NotEmptyTrimmed();
        RuleFor(request => request.PageSize).PageSize(1, 10);
        RuleFor(request => request.Email).OptionalEmailAddress();
    }
}

internal sealed class MultiFailureValidator : AbstractValidatorBase<FakeRequest>
{
    public MultiFailureValidator()
    {
        RuleFor(request => request.Name).NotEmptyTrimmed("VAL.EMPTY.NAME", "Name is required.");
        RuleFor(request => request.PageSize).PageSize(5, 6, "VAL.PAGE_SIZE.RANGE", "Page size must be between 5 and 6.");
    }
}

internal sealed class MixedSeverityValidator : AbstractValidatorBase<FakeRequest>
{
    public MixedSeverityValidator()
    {
        RuleFor(request => request.Name).NotEmpty().WithSeverity(Severity.Warning);
        RuleFor(request => request.PageSize).GreaterThan(5).WithSeverity(Severity.Info);
    }
}

internal sealed class InfoSeverityValidator : AbstractValidatorBase<FakeRequest>
{
    public InfoSeverityValidator()
    {
        RuleFor(request => request.Name).NotEmpty().WithSeverity(Severity.Info);
    }
}

internal sealed class RecordingPublisher : IValidationNotificationPublisher
{
    public int PublishCount { get; private set; }

    public IReadOnlyList<ValidationError>? LastErrors { get; private set; }

    public Task PublishAsync(IReadOnlyList<ValidationError> errors, CancellationToken cancellationToken = default)
    {
        PublishCount++;
        LastErrors = errors;
        return Task.CompletedTask;
    }
}
