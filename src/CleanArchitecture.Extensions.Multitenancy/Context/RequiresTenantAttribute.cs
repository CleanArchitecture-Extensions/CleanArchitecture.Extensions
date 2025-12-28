using CleanArchitecture.Extensions.Multitenancy.Abstractions;

namespace CleanArchitecture.Extensions.Multitenancy;

/// <summary>
/// Marks a request or endpoint as requiring a tenant.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class RequiresTenantAttribute : Attribute, ITenantRequirement
{
    /// <inheritdoc />
    public TenantRequirementMode Requirement => TenantRequirementMode.Required;
}
