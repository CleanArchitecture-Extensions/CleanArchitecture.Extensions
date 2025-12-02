using FluentValidation;

namespace CleanArchitecture.Extensions.Validation.Validators;

/// <summary>
/// Base validator that applies shared conventions for CleanArchitecture.Extensions.
/// </summary>
/// <typeparam name="T">Type being validated.</typeparam>
public abstract class AbstractValidatorBase<T> : AbstractValidator<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractValidatorBase{T}"/> class.
    /// </summary>
    protected AbstractValidatorBase()
    {
        // Default to fail-fast per rule chain while allowing other rules to execute.
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Continue;
    }
}
