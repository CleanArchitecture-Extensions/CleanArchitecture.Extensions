namespace CleanArchitecture.Extensions.Multitenancy.Abstractions;

/// <summary>
/// Serializes tenant context for background jobs and message metadata.
/// </summary>
public interface ITenantContextSerializer
{
    /// <summary>
    /// Serializes the tenant context into a string payload.
    /// </summary>
    /// <param name="context">Tenant context.</param>
    /// <returns>Serialized payload.</returns>
    string Serialize(TenantContext context);

    /// <summary>
    /// Deserializes a tenant context payload.
    /// </summary>
    /// <param name="payload">Serialized payload.</param>
    /// <returns>Tenant context.</returns>
    TenantContext Deserialize(string payload);
}
