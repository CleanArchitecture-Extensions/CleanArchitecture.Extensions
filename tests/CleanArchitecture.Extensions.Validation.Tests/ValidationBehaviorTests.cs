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

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_NoValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<FakeRequest, Result>(
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
        var behavior = new ValidationBehavior<FakeRequest, Result>(new[] { validator }, MicrosoftOptions.Create(ValidationOptions.Default));

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
        var behavior = new ValidationBehavior<FakeRequest, Result>(new[] { validator }, MicrosoftOptions.Create(options));
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
        var behavior = new ValidationBehavior<FakeRequest, Result>(new[] { validator }, options);

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
        var behavior = new ValidationBehavior<FakeRequest, Result>(new[] { validator }, options, publisher);

        var result = await behavior.Handle(new FakeRequest(string.Empty, 0, "bad-email"), _ => Task.FromResult(Result.Success()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(1, publisher.PublishCount);
        Assert.NotNull(publisher.LastErrors);
        Assert.NotEmpty(publisher.LastErrors!);
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
