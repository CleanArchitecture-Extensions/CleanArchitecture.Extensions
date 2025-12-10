namespace CleanArchitecture.Extensions.Caching.Options;

/// <summary>
/// Configures caching defaults for the extensions package.
/// </summary>
public sealed class CachingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether caching is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the default namespace applied to cache keys to avoid collisions across applications.
    /// </summary>
    public string DefaultNamespace { get; set; } = "CleanArchitectureExtensions";

    /// <summary>
    /// Gets the default options instance.
    /// </summary>
    public static CachingOptions Default => new();
}
