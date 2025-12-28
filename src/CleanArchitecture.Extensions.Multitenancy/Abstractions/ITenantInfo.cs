namespace CleanArchitecture.Extensions.Multitenancy.Abstractions;

/// <summary>
/// Represents tenant identity and lifecycle metadata.
/// </summary>
public interface ITenantInfo
{
    /// <summary>
    /// Gets the external tenant identifier.
    /// </summary>
    string TenantId { get; }

    /// <summary>
    /// Gets the internal tenant identifier, if available.
    /// </summary>
    Guid? InternalId { get; }

    /// <summary>
    /// Gets the tenant display name.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Gets the tenant classification (for example, free/paid/partner).
    /// </summary>
    string? Type { get; }

    /// <summary>
    /// Gets the tenant region or residency tag.
    /// </summary>
    string? Region { get; }

    /// <summary>
    /// Gets a value indicating whether the tenant is active.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Gets a value indicating whether the tenant is soft-deleted.
    /// </summary>
    bool IsSoftDeleted { get; }

    /// <summary>
    /// Gets the tenant creation timestamp.
    /// </summary>
    DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets the tenant expiration timestamp, if applicable.
    /// </summary>
    DateTimeOffset? ExpiresAt { get; }

    /// <summary>
    /// Gets the parent tenant identifier, when tenants are hierarchical.
    /// </summary>
    string? ParentId { get; }

    /// <summary>
    /// Gets the tenant lifecycle state.
    /// </summary>
    TenantState State { get; }

    /// <summary>
    /// Gets tenant metadata values.
    /// </summary>
    IReadOnlyDictionary<string, string> Metadata { get; }
}
