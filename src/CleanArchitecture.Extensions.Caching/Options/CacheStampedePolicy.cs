namespace CleanArchitecture.Extensions.Caching.Options;

/// <summary>
/// Configures protections against cache stampede scenarios.
/// </summary>
public sealed class CacheStampedePolicy
{
    /// <summary>
    /// Gets or sets a value indicating whether stampede protection (locking) is enabled.
    /// </summary>
    public bool EnableLocking { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum duration to wait for a concurrent factory to complete.
    /// </summary>
    public TimeSpan LockTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets the jitter applied to expiration to avoid synchronized cache invalidation.
    /// </summary>
    public TimeSpan? Jitter { get; set; } = TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// Gets the default stampede policy.
    /// </summary>
    public static CacheStampedePolicy Default => new();
}
