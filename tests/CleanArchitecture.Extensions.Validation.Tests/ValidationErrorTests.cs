using CleanArchitecture.Extensions.Core.Results;
using CleanArchitecture.Extensions.Validation.Models;
using CleanArchitecture.Extensions.Validation.Options;
using FluentValidation;
using FluentValidation.Results;

namespace CleanArchitecture.Extensions.Validation.Tests;

/// <summary>
/// Tests covering mapping between FluentValidation failures and core errors.
/// </summary>
public class ValidationErrorTests
{
    [Fact]
    public void FromFailure_IncludesPropertyAttemptedValueAndMetadata_WhenEnabled()
    {
        var failure = new ValidationFailure("Name", "required")
        {
            ErrorCode = "",
            Severity = Severity.Warning,
            AttemptedValue = 5,
            FormattedMessagePlaceholderValues = new Dictionary<string, object?>
            {
                ["Length"] = 10
            }
        };
        var options = new ValidationOptions
        {
            IncludePropertyName = true,
            IncludeAttemptedValue = true,
            IncludePlaceholderValues = true,
            DefaultErrorCode = "VAL.DEFAULT"
        };

        var error = ValidationError.FromFailure(failure, options);

        Assert.Equal("VAL.DEFAULT", error.Code);
        Assert.Equal("required", error.Message);
        Assert.Equal("Name", error.PropertyName);
        Assert.Equal("5", error.AttemptedValue);
        Assert.Equal(Severity.Warning.ToString(), error.Severity);
        Assert.Equal("10", error.Metadata["Length"]);
    }

    [Fact]
    public void FromFailure_UsesSelectorsWhenProvided()
    {
        var failure = new ValidationFailure("Field", "oops") { ErrorCode = "VAL.CODE" };
        var options = new ValidationOptions
        {
            ErrorCodeSelector = f => $"{f.ErrorCode}.SELECTED",
            MessageFormatter = f => $"Formatted-{f.ErrorMessage}"
        };

        var error = ValidationError.FromFailure(failure, options);

        Assert.Equal("VAL.CODE.SELECTED", error.Code);
        Assert.Equal("Formatted-oops", error.Message);
    }

    [Fact]
    public void ToCoreError_MergesMetadataPropertyAttemptedValueAndSeverity()
    {
        var validationError = new ValidationError(
            "VAL.CODE",
            "message",
            propertyName: "Field",
            attemptedValue: "value",
            severity: Severity.Error.ToString(),
            metadata: new Dictionary<string, string> { ["extra"] = "meta" });

        var core = validationError.ToCoreError("trace-1");

        Assert.Equal("trace-1", core.TraceId);
        Assert.Equal("meta", core.Metadata["extra"]);
        Assert.Equal("Field", core.Metadata["property"]);
        Assert.Equal("value", core.Metadata["attemptedValue"]);
        Assert.Equal(Severity.Error.ToString(), core.Metadata["severity"]);
    }
}
