namespace CleanArchitecture.Extensions.Multitenancy.Abstractions;

/// <summary>
/// Provides access to the current tenant context.
/// </summary>
public interface ICurrentTenant
{
    /// <summary>
    /// Gets the current tenant identifier, if resolved.
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// Gets the current tenant information, if available.
    /// </summary>
    ITenantInfo? TenantInfo { get; }

    /// <summary>
    /// Gets the current tenant context, if resolved.
    /// </summary>
    TenantContext? Context { get; }

    /// <summary>
    /// Gets a value indicating whether a tenant identifier was resolved.
    /// </summary>
    bool IsResolved { get; }

    /// <summary>
    /// Gets a value indicating whether the tenant has been validated.
    /// </summary>
    bool IsValidated { get; }

    /// <summary>
    /// Gets the resolution source, if available.
    /// </summary>
    TenantResolutionSource? Source { get; }

    /// <summary>
    /// Gets the resolution confidence.
    /// </summary>
    TenantResolutionConfidence Confidence { get; }
}
