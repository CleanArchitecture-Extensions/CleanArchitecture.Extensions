namespace CleanArchitecture.Extensions.Multitenancy.Abstractions;

/// <summary>
/// Resolves tenant identifiers from a specific source.
/// </summary>
public interface ITenantProvider
{
    /// <summary>
    /// Gets the resolution source handled by the provider.
    /// </summary>
    TenantResolutionSource Source { get; }

    /// <summary>
    /// Attempts to resolve a tenant identifier.
    /// </summary>
    /// <param name="context">Resolution context populated by the host environment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Resolution result.</returns>
    ValueTask<TenantResolutionResult> ResolveAsync(TenantResolutionContext context, CancellationToken cancellationToken = default);
}
