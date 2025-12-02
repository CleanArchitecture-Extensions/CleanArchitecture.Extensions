namespace CleanArchitecture.Extensions.Validation.Options;

/// <summary>
/// Determines how validation failures are handled by the pipeline behavior.
/// </summary>
public enum ValidationStrategy
{
    /// <summary>
    /// Throw a ValidationException (default Clean Architecture template behavior).
    /// </summary>
    Throw = 0,

    /// <summary>
    /// Short-circuit and return a Result failure when the handler returns Result/Result&lt;T&gt;.
    /// </summary>
    ReturnResult = 1,

    /// <summary>
    /// Publish notifications for failures and then either throw or return a Result based on NotifyBehavior.
    /// </summary>
    Notify = 2
}
