using CleanArchitecture.Extensions.Multitenancy.Abstractions;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Attributes;

/// <summary>
/// Marks an endpoint as allowing tenant-less requests.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class AllowAnonymousTenantAttribute : Attribute, ITenantRequirement
{
    /// <inheritdoc />
    public TenantRequirementMode Requirement => TenantRequirementMode.Optional;
}
