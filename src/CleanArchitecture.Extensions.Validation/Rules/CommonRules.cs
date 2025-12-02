using FluentValidation;

namespace CleanArchitecture.Extensions.Validation.Rules;

/// <summary>
/// Shared validation rule helpers for common patterns (ids, paging, email).
/// </summary>
public static class CommonRules
{
    /// <summary>
    /// Ensures a string is not null, empty, or whitespace after trimming.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> NotEmptyTrimmed<T>(this IRuleBuilder<T, string?> ruleBuilder, string errorCode = "VAL.EMPTY", string? message = null) =>
        ruleBuilder
            .NotEmpty()
            .WithMessage(message ?? "Value cannot be empty.")
            .WithErrorCode(errorCode)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage(message ?? "Value cannot be empty.")
            .WithErrorCode(errorCode);

    /// <summary>
    /// Ensures an email address is well-formed.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> EmailAddressBasic<T>(this IRuleBuilder<T, string?> ruleBuilder, string errorCode = "VAL.EMAIL", string? message = null) =>
        ruleBuilder
            .NotEmpty()
            .EmailAddress()
            .WithMessage(message ?? "Email address is not valid.")
            .WithErrorCode(errorCode);

    /// <summary>
    /// Allows an optional email address while enforcing format when present.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> OptionalEmailAddress<T>(this IRuleBuilder<T, string?> ruleBuilder, string errorCode = "VAL.EMAIL", string? message = null) =>
        ruleBuilder
            .Must(value => string.IsNullOrWhiteSpace(value) || new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(value))
            .WithMessage(message ?? "Email address is not valid.")
            .WithErrorCode(errorCode);

    /// <summary>
    /// Ensures a positive identifier.
    /// </summary>
    public static IRuleBuilderOptions<T, int> PositiveId<T>(this IRuleBuilder<T, int> ruleBuilder, string errorCode = "VAL.ID.POSITIVE", string? message = null) =>
        ruleBuilder
            .GreaterThan(0)
            .WithMessage(message ?? "Identifier must be greater than zero.")
            .WithErrorCode(errorCode);

    /// <summary>
    /// Ensures a page number is greater than or equal to the minimum (default 1).
    /// </summary>
    public static IRuleBuilderOptions<T, int> PageNumber<T>(this IRuleBuilder<T, int> ruleBuilder, int minimum = 1, string errorCode = "VAL.PAGE", string? message = null) =>
        ruleBuilder
            .GreaterThanOrEqualTo(minimum)
            .WithMessage(message ?? $"Page number must be at least {minimum}.")
            .WithErrorCode(errorCode);

    /// <summary>
    /// Ensures a page size is within an inclusive range.
    /// </summary>
    public static IRuleBuilderOptions<T, int> PageSize<T>(this IRuleBuilder<T, int> ruleBuilder, int minimum = 1, int maximum = 200, string errorCode = "VAL.PAGE_SIZE", string? message = null) =>
        ruleBuilder
            .InclusiveBetween(minimum, maximum)
            .WithMessage(message ?? $"Page size must be between {minimum} and {maximum}.")
            .WithErrorCode(errorCode);
}
