using FluentValidation.Results;

namespace CleanArchitecture.Extensions.Validation.Options;

/// <summary>
/// Configures how validation is executed and surfaced in the pipeline.
/// </summary>
public sealed class ValidationOptions
{
    /// <summary>
    /// Gets or sets the handling strategy for validation failures.
    /// </summary>
    public ValidationStrategy Strategy { get; set; } = ValidationStrategy.Throw;

    /// <summary>
    /// Gets or sets the action taken after notifications are published when <see cref="Strategy"/> is Notify.
    /// </summary>
    public ValidationNotifyBehavior NotifyBehavior { get; set; } = ValidationNotifyBehavior.ReturnResult;

    /// <summary>
    /// Gets or sets the maximum number of failures to surface (helps avoid large payloads).
    /// </summary>
    public int MaxFailures { get; set; } = 50;

    /// <summary>
    /// Gets or sets a value indicating whether property names are included in surfaced errors.
    /// </summary>
    public bool IncludePropertyName { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether attempted values are included in surfaced errors.
    /// </summary>
    public bool IncludeAttemptedValue { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether placeholder values from FluentValidation are included in metadata.
    /// </summary>
    public bool IncludePlaceholderValues { get; set; }

    /// <summary>
    /// Gets or sets the default error code applied when validators do not provide one.
    /// </summary>
    public string DefaultErrorCode { get; set; } = "validation.failure";

    /// <summary>
    /// Gets or sets the optional trace identifier to apply to errors/results.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Gets or sets a delegate used to derive the error code from a validation failure.
    /// </summary>
    public Func<ValidationFailure, string>? ErrorCodeSelector { get; set; }

    /// <summary>
    /// Gets or sets a delegate used to produce the final error message for a failure.
    /// </summary>
    public Func<ValidationFailure, string>? MessageFormatter { get; set; }

    /// <summary>
    /// Gets the default options instance.
    /// </summary>
    public static ValidationOptions Default => new();
}
