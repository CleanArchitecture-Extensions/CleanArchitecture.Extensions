using CleanArchitecture.Extensions.Validation.Validators;
using FluentValidation;

namespace CleanArchitecture.Extensions.Validation.Tests;

/// <summary>
/// Tests covering validator base defaults.
/// </summary>
public class AbstractValidatorBaseTests
{
    [Fact]
    public void Defaults_SetExpectedCascadeModes()
    {
        var validator = new SampleValidator();

        Assert.Equal(CascadeMode.Stop, validator.RuleLevelCascadeMode);
        Assert.Equal(CascadeMode.Continue, validator.ClassLevelCascadeMode);
    }

    private sealed class Sample
    {
        public string? Name { get; set; }
    }

    private sealed class SampleValidator : AbstractValidatorBase<Sample>
    {
        public SampleValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}
