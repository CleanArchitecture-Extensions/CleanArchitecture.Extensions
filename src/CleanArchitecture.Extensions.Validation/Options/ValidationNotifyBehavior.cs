namespace CleanArchitecture.Extensions.Validation.Options;

/// <summary>
/// Defines the action taken after validation notifications are published.
/// </summary>
public enum ValidationNotifyBehavior
{
    /// <summary>
    /// Return a Result failure (when supported) after publishing notifications.
    /// </summary>
    ReturnResult = 0,

    /// <summary>
    /// Throw a ValidationException after publishing notifications.
    /// </summary>
    Throw = 1
}
