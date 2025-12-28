namespace CleanArchitecture.Extensions.Multitenancy.Configuration;

/// <summary>
/// Specifies validation behavior for resolved tenants.
/// </summary>
public enum TenantValidationMode
{
    /// <summary>
    /// No validation is performed.
    /// </summary>
    None,

    /// <summary>
    /// Validate against cache only.
    /// </summary>
    Cache,

    /// <summary>
    /// Validate against the repository/store (optionally caching results).
    /// </summary>
    Repository
}
