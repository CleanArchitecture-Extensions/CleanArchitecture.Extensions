namespace CleanArchitecture.Extensions.Multitenancy.Exceptions;

/// <summary>
/// Indicates that a resolved tenant was not found.
/// </summary>
public sealed class TenantNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TenantNotFoundException"/> class.
    /// </summary>
    /// <param name="tenantId">Tenant identifier.</param>
    public TenantNotFoundException(string? tenantId)
        : base(BuildMessage(tenantId))
    {
        TenantId = tenantId;
    }

    /// <summary>
    /// Gets the tenant identifier that was not found.
    /// </summary>
    public string? TenantId { get; }

    private static string BuildMessage(string? tenantId) =>
        string.IsNullOrWhiteSpace(tenantId)
            ? "Tenant could not be found."
            : $"Tenant '{tenantId}' could not be found.";
}
