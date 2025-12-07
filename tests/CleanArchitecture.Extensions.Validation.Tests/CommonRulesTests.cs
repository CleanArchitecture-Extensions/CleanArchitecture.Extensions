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
