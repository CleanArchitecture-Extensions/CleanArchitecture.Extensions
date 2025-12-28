using CleanArchitecture.Extensions.Multitenancy.Abstractions;

namespace CleanArchitecture.Extensions.Multitenancy;

/// <summary>
/// Represents a resolved tenant context.
/// </summary>
public sealed class TenantContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TenantContext"/> class.
    /// </summary>
    /// <param name="tenant">Resolved tenant information.</param>
    /// <param name="resolution">Resolution metadata.</param>
    /// <param name="correlationId">Optional correlation identifier.</param>
    /// <param name="isValidated">Whether tenant information is validated.</param>
    public TenantContext(
        ITenantInfo tenant,
        TenantResolutionResult resolution,
        string? correlationId = null,
        bool isValidated = true)
    {
        Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        CorrelationId = correlationId;
        IsValidated = isValidated;
    }

    /// <summary>
    /// Gets or sets the tenant information.
    /// </summary>
    public ITenantInfo Tenant { get; set; }

    /// <summary>
    /// Gets or sets the resolution metadata.
    /// </summary>
    public TenantResolutionResult Resolution { get; set; }

    /// <summary>
    /// Gets or sets the correlation identifier.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the tenant was resolved.
    /// </summary>
    public DateTimeOffset ResolvedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets a value indicating whether the tenant has been validated.
    /// </summary>
    public bool IsValidated { get; set; }

    /// <summary>
    /// Gets the resolved tenant identifier.
    /// </summary>
    public string TenantId => Tenant.TenantId;

    /// <summary>
    /// Gets the resolution source.
    /// </summary>
    public TenantResolutionSource Source => Resolution.Source;

    /// <summary>
    /// Gets the resolution confidence.
    /// </summary>
    public TenantResolutionConfidence Confidence => Resolution.Confidence;
}
