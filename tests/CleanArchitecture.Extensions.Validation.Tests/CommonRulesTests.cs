using CleanArchitecture.Extensions.Validation.Rules;
using CleanArchitecture.Extensions.Validation.Validators;
using FluentValidation;

namespace CleanArchitecture.Extensions.Validation.Tests;

public sealed record RuleSample(string? Phone, string? Url, string? Culture, string? Sort);

public class CommonRulesTests
{
    [Fact]
    public void PhoneE164_AllowsNull_WhenAllowEmpty()
    {
        var validator = new RuleSampleValidator();

        var result = validator.Validate(new RuleSample(null, null, null, null));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void PhoneE164_RejectsNonE164()
    {
        var validator = new RuleSampleValidator();

        var result = validator.Validate(new RuleSample("123-456", null, null, null));

        Assert.Contains(result.Errors, e => e.ErrorCode == "VAL.PHONE");
    }

    [Fact]
    public void UrlAbsoluteHttpHttps_RejectsNonHttp()
    {
        var validator = new RuleSampleValidator();

        var result = validator.Validate(new RuleSample(null, "ftp://example.com", null, null));

        Assert.Contains(result.Errors, e => e.ErrorCode == "VAL.URL");
    }

    [Fact]
    public void CultureCode_ValidatesExistingCultures()
    {
        var validator = new RuleSampleValidator();

        var result = validator.Validate(new RuleSample(null, null, "en-US", null));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void SortExpression_RejectsUnsupportedField()
    {
        var validator = new RuleSampleValidator();

        var result = validator.Validate(new RuleSample(null, null, null, "unknown desc"));

        Assert.Contains(result.Errors, e => e.ErrorCode == "VAL.SORT");
    }

    [Fact]
    public void SortExpression_AllowsKnownFieldsAndDirections()
    {
        var validator = new RuleSampleValidator();

        var result = validator.Validate(new RuleSample(null, null, null, "name asc, created desc"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void EmailAddressBasic_RejectsInvalidFormat()
    {
        var validator = new InlineValidator<RuleSample>();
        validator.RuleFor(x => x.Phone).EmailAddressBasic();

        var result = validator.Validate(new RuleSample("not-an-email", null, null, null));

        Assert.Contains(result.Errors, e => e.ErrorCode == "VAL.EMAIL");
    }

    [Fact]
    public void OptionalEmailAddress_AllowsEmptyButRejectsInvalid()
    {
        var validator = new InlineValidator<RuleSample>();
        validator.RuleFor(x => x.Phone).OptionalEmailAddress();

        var emptyResult = validator.Validate(new RuleSample(null, null, null, null));
        var invalidResult = validator.Validate(new RuleSample("not-an-email", null, null, null));

        Assert.True(emptyResult.IsValid);
        Assert.Contains(invalidResult.Errors, e => e.ErrorCode == "VAL.EMAIL");
    }

    [Fact]
    public void PositiveId_RejectsNonPositiveValues()
    {
        var validator = new InlineValidator<int>();
        validator.RuleFor(x => x).PositiveId();

        var result = validator.Validate(0);

        Assert.Contains(result.Errors, e => e.ErrorCode == "VAL.ID.POSITIVE");
    }

    [Fact]
    public void PageNumber_RejectsBelowMinimum()
    {
        var validator = new InlineValidator<int>();
        validator.RuleFor(x => x).PageNumber(2);

        var result = validator.Validate(1);

        Assert.Contains(result.Errors, e => e.ErrorCode == "VAL.PAGE");
    }

    [Fact]
    public void PageSize_RejectsOutsideRange()
    {
        var validator = new InlineValidator<int>();
        validator.RuleFor(x => x).PageSize(1, 5);

        var result = validator.Validate(10);

        Assert.Contains(result.Errors, e => e.ErrorCode == "VAL.PAGE_SIZE");
    }

    [Fact]
    public void CultureCode_RejectsNeutralWhenDisabled()
    {
        var validator = new InlineValidator<RuleSample>();
        validator.RuleFor(x => x.Culture).CultureCode(allowNeutral: false);

        var result = validator.Validate(new RuleSample(null, null, "en", null));

        Assert.Contains(result.Errors, e => e.ErrorCode == "VAL.CULTURE");
    }

    [Fact]
    public void SortExpression_RejectsWhenAllowedFieldsEmpty()
    {
        var validator = new InlineValidator<RuleSample>();
        validator.RuleFor(x => x.Sort).SortExpression(Array.Empty<string>());

        var result = validator.Validate(new RuleSample(null, null, null, "name"));

        Assert.Contains(result.Errors, e => e.ErrorCode == "VAL.SORT");
    }

    private sealed class RuleSampleValidator : AbstractValidatorBase<RuleSample>
    {
        public RuleSampleValidator()
        {
            RuleFor(x => x.Phone).PhoneE164();
            RuleFor(x => x.Url).UrlAbsoluteHttpHttps();
            RuleFor(x => x.Culture).CultureCode();
            RuleFor(x => x.Sort).SortExpression(new[] { "name", "created" });
        }
    }
}
