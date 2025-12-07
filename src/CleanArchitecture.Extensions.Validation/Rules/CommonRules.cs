using System.Globalization;
using System.Text.RegularExpressions;
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

    /// <summary>
    /// Ensures a phone number uses a simple E.164 format (+[country][number]).
    /// </summary>
    public static IRuleBuilderOptions<T, string?> PhoneE164<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string errorCode = "VAL.PHONE",
        string? message = null,
        bool allowEmpty = true) =>
        ruleBuilder
            .Must(value =>
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return allowEmpty;
                }

                return Regex.IsMatch(value, @"^\+[1-9]\d{7,14}$");
            })
            .WithMessage(message ?? "Phone number must be in E.164 format.")
            .WithErrorCode(errorCode);

    /// <summary>
    /// Ensures a URL is absolute and uses HTTP or HTTPS.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> UrlAbsoluteHttpHttps<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string errorCode = "VAL.URL",
        string? message = null,
        bool allowEmpty = true) =>
        ruleBuilder
            .Must(value =>
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return allowEmpty;
                }

                return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
                       (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
            })
            .WithMessage(message ?? "URL must be absolute and use http or https.")
            .WithErrorCode(errorCode);

    /// <summary>
    /// Ensures a culture code exists. Defaults to allowing both specific (en-US) and neutral (en) cultures.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> CultureCode<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string errorCode = "VAL.CULTURE",
        string? message = null,
        bool allowEmpty = true,
        bool allowNeutral = true) =>
        ruleBuilder
            .Must(value =>
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return allowEmpty;
                }

                try
                {
                    var culture = CultureInfo.GetCultureInfo(value);
                    return allowNeutral || culture.Name.Contains('-');
                }
                catch (CultureNotFoundException)
                {
                    return false;
                }
            })
            .WithMessage(message ?? "Culture code is not valid.")
            .WithErrorCode(errorCode);

    /// <summary>
    /// Ensures a sort expression contains only allowed fields and directions (asc/desc).
    /// </summary>
    public static IRuleBuilderOptions<T, string?> SortExpression<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        IEnumerable<string> allowedFields,
        string errorCode = "VAL.SORT",
        string? message = null,
        bool allowEmpty = true) =>
        ruleBuilder
            .Must(value =>
            {
                if (allowedFields is null)
                {
                    return false;
                }

                var allowed = allowedFields.ToHashSet(StringComparer.OrdinalIgnoreCase);
                if (allowed.Count == 0)
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(value))
                {
                    return allowEmpty;
                }

                var terms = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var term in terms)
                {
                    var parts = term.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (parts.Length == 0 || parts.Length > 2)
                    {
                        return false;
                    }

                    var field = parts[0];
                    if (!allowed.Contains(field))
                    {
                        return false;
                    }

                    if (parts.Length == 2)
                    {
                        var dir = parts[1];
                        if (!dir.Equals("asc", StringComparison.OrdinalIgnoreCase) &&
                            !dir.Equals("desc", StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                    }
                }

                return true;
            })
            .WithMessage(message ?? "Sort expression is invalid or contains unsupported fields.")
            .WithErrorCode(errorCode);
}
