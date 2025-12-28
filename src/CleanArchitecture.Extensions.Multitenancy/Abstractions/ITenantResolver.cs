namespace CleanArchitecture.Extensions.Multitenancy.Abstractions;

/// <summary>
/// Resolves tenant context using registered providers and validation policies.
/// </summary>
public interface ITenantResolver
{
    /// <summary>
    /// Resolves a tenant context from the supplied resolution context.
    /// </summary>
    /// <param name="context">Resolution context populated by the host environment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Resolved tenant context or null when no tenant could be determined.</returns>
    Task<TenantContext?> ResolveAsync(TenantResolutionContext context, CancellationToken cancellationToken = default);
}
