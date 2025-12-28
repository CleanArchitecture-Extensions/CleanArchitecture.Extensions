using CleanArchitecture.Extensions.Multitenancy.Abstractions;

namespace CleanArchitecture.Extensions.Multitenancy;

/// <summary>
/// Marks a request or endpoint as allowing host-level access without a tenant.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class AllowHostRequestsAttribute : Attribute, ITenantRequirement
{
    /// <inheritdoc />
    public TenantRequirementMode Requirement => TenantRequirementMode.Optional;
}
