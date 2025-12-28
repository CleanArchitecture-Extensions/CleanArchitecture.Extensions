namespace CleanArchitecture.Extensions.Multitenancy.Abstractions;

/// <summary>
/// Defines the strategy for ordering and evaluating tenant resolution providers.
/// </summary>
public interface ITenantResolutionStrategy
{
    /// <summary>
    /// Resolves a tenant identifier using registered providers.
    /// </summary>
    /// <param name="context">Resolution context populated by the host environment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Resolution result.</returns>
    Task<TenantResolutionResult> ResolveAsync(TenantResolutionContext context, CancellationToken cancellationToken = default);
}
