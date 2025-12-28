namespace CleanArchitecture.Extensions.Multitenancy;

/// <summary>
/// Indicates confidence level for a tenant resolution result.
/// </summary>
public enum TenantResolutionConfidence
{
    /// <summary>
    /// No confidence / not resolved.
    /// </summary>
    None,

    /// <summary>
    /// Low confidence.
    /// </summary>
    Low,

    /// <summary>
    /// Medium confidence.
    /// </summary>
    Medium,

    /// <summary>
    /// High confidence.
    /// </summary>
    High
}
