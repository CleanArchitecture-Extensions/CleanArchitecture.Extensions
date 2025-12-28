namespace CleanArchitecture.Extensions.Multitenancy.Abstractions;

/// <summary>
/// Declares the tenant requirement for a request or endpoint.
/// </summary>
public interface ITenantRequirement
{
    /// <summary>
    /// Gets the tenant requirement mode.
    /// </summary>
    TenantRequirementMode Requirement { get; }
}
