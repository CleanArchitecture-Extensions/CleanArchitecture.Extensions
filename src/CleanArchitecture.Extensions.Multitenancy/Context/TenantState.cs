namespace CleanArchitecture.Extensions.Multitenancy;

/// <summary>
/// Represents tenant lifecycle state.
/// </summary>
public enum TenantState
{
    /// <summary>
    /// Tenant state is unknown.
    /// </summary>
    Unknown,

    /// <summary>
    /// Tenant is active.
    /// </summary>
    Active,

    /// <summary>
    /// Tenant is suspended.
    /// </summary>
    Suspended,

    /// <summary>
    /// Tenant is pending provisioning.
    /// </summary>
    PendingProvision,

    /// <summary>
    /// Tenant is deleted.
    /// </summary>
    Deleted
}
