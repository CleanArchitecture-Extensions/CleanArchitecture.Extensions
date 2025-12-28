namespace CleanArchitecture.Extensions.Multitenancy;

/// <summary>
/// Identifies where a tenant resolution value originated.
/// </summary>
public enum TenantResolutionSource
{
    /// <summary>
    /// Unknown or unspecified source.
    /// </summary>
    Unknown,

    /// <summary>
    /// Route parameter source.
    /// </summary>
    Route,

    /// <summary>
    /// Hostname source.
    /// </summary>
    Host,

    /// <summary>
    /// Header source.
    /// </summary>
    Header,

    /// <summary>
    /// Query string source.
    /// </summary>
    QueryString,

    /// <summary>
    /// Claim source.
    /// </summary>
    Claim,

    /// <summary>
    /// Default or fallback source.
    /// </summary>
    Default,

    /// <summary>
    /// Custom provider source.
    /// </summary>
    Custom,

    /// <summary>
    /// Composite strategy source.
    /// </summary>
    Composite
}
