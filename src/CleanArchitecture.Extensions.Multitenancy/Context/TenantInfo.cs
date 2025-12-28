using CleanArchitecture.Extensions.Multitenancy.Abstractions;

namespace CleanArchitecture.Extensions.Multitenancy;

/// <summary>
/// Default tenant information model.
/// </summary>
public sealed class TenantInfo : ITenantInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TenantInfo"/> class.
    /// </summary>
    public TenantInfo()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantInfo"/> class with the specified identifier.
    /// </summary>
    /// <param name="tenantId">Tenant identifier.</param>
    public TenantInfo(string tenantId)
    {
        TenantId = string.IsNullOrWhiteSpace(tenantId)
            ? throw new ArgumentException("Tenant identifier cannot be empty.", nameof(tenantId))
            : tenantId.Trim();
    }

    /// <inheritdoc />
    public string TenantId { get; set; } = string.Empty;

    /// <inheritdoc />
    public Guid? InternalId { get; set; }

    /// <inheritdoc />
    public string? Name { get; set; }

    /// <inheritdoc />
    public string? Type { get; set; }

    /// <inheritdoc />
    public string? Region { get; set; }

    /// <inheritdoc />
    public bool IsActive { get; set; } = true;

    /// <inheritdoc />
    public bool IsSoftDeleted { get; set; }

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <inheritdoc />
    public string? ParentId { get; set; }

    /// <inheritdoc />
    public TenantState State { get; set; } = TenantState.Active;

    /// <summary>
    /// Gets or sets the tenant metadata values.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    IReadOnlyDictionary<string, string> ITenantInfo.Metadata => Metadata;

    /// <summary>
    /// Creates a <see cref="TenantInfo"/> snapshot from another tenant info implementation.
    /// </summary>
    /// <param name="tenant">Tenant info source.</param>
    /// <returns>Tenant info snapshot.</returns>
    public static TenantInfo From(ITenantInfo tenant)
    {
        ArgumentNullException.ThrowIfNull(tenant);

        return new TenantInfo
        {
            TenantId = tenant.TenantId,
            InternalId = tenant.InternalId,
            Name = tenant.Name,
            Type = tenant.Type,
            Region = tenant.Region,
            IsActive = tenant.IsActive,
            IsSoftDeleted = tenant.IsSoftDeleted,
            CreatedAt = tenant.CreatedAt,
            ExpiresAt = tenant.ExpiresAt,
            ParentId = tenant.ParentId,
            State = tenant.State,
            Metadata = new Dictionary<string, string>(tenant.Metadata, StringComparer.OrdinalIgnoreCase)
        };
    }
}
