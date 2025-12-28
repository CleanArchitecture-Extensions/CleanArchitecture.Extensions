namespace CleanArchitecture.Extensions.Multitenancy.Abstractions;

/// <summary>
/// Describes whether a request requires a tenant.
/// </summary>
public enum TenantRequirementMode
{
    /// <summary>
    /// Tenant is required for the operation.
    /// </summary>
    Required,

    /// <summary>
    /// Tenant is optional (host-level requests are allowed).
    /// </summary>
    Optional
}
