using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.Providers;

/// <summary>
/// Composite strategy that evaluates providers in a configured order.
/// </summary>
public sealed class CompositeTenantResolutionStrategy : ITenantResolutionStrategy
{
    private readonly IReadOnlyList<ITenantProvider> _providers;
    private readonly MultitenancyOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeTenantResolutionStrategy"/> class.
    /// </summary>
    /// <param name="providers">Registered tenant providers.</param>
    /// <param name="options">Multitenancy options.</param>
    public CompositeTenantResolutionStrategy(
        IEnumerable<ITenantProvider> providers,
        IOptions<MultitenancyOptions> options)
    {
        _providers = providers?.ToList() ?? throw new ArgumentNullException(nameof(providers));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public async Task<TenantResolutionResult> ResolveAsync(
        TenantResolutionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        using var cts = CreateTimeoutToken(cancellationToken);
        var token = cts?.Token ?? cancellationToken;

        var orderedProviders = OrderProviders();

        if (_options.RequireMatchAcrossSources)
        {
            return await ResolveWithConsensusAsync(orderedProviders, context, token).ConfigureAwait(false);
        }

        TenantResolutionResult? ambiguous = null;
        foreach (var provider in orderedProviders)
        {
            token.ThrowIfCancellationRequested();

            var result = await provider.ResolveAsync(context, token).ConfigureAwait(false);
            if (result.IsResolved)
            {
                return result;
            }

            if (result.IsAmbiguous && ambiguous is null)
            {
                ambiguous = result;
            }
        }

        return ambiguous ?? TenantResolutionResult.NotFound(TenantResolutionSource.Composite);
    }

    private async Task<TenantResolutionResult> ResolveWithConsensusAsync(
        IReadOnlyList<ITenantProvider> orderedProviders,
        TenantResolutionContext context,
        CancellationToken cancellationToken)
    {
        var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var fallbackCandidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var provider in orderedProviders)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await provider.ResolveAsync(context, cancellationToken).ConfigureAwait(false);
            var target = provider.Source == TenantResolutionSource.Default
                ? fallbackCandidates
                : candidates;
            foreach (var candidate in result.Candidates)
            {
                target.Add(candidate);
            }
        }

        var resolvedCandidates = candidates.Count > 0 ? candidates : fallbackCandidates;
        var resolvedFromFallback = candidates.Count == 0 && fallbackCandidates.Count > 0;

        if (resolvedCandidates.Count == 0)
        {
            return TenantResolutionResult.NotFound(TenantResolutionSource.Composite);
        }

        var source = resolvedFromFallback ? TenantResolutionSource.Default : TenantResolutionSource.Composite;

        if (resolvedCandidates.Count == 1)
        {
            var tenantId = resolvedCandidates.First();
            var confidence = resolvedFromFallback ? TenantResolutionConfidence.Low : TenantResolutionConfidence.Medium;
            return TenantResolutionResult.Resolved(tenantId, source, confidence);
        }

        return TenantResolutionResult.FromCandidates(resolvedCandidates, source, TenantResolutionConfidence.Low);
    }

    private IReadOnlyList<ITenantProvider> OrderProviders()
    {
        if (_providers.Count == 0)
        {
            return Array.Empty<ITenantProvider>();
        }

        var providersBySource = _providers
            .GroupBy(provider => provider.Source)
            .ToDictionary(group => group.Key, group => group.ToList());

        var ordered = new List<ITenantProvider>();
        var assigned = new HashSet<ITenantProvider>();

        foreach (var source in _options.ResolutionOrder)
        {
            if (!providersBySource.TryGetValue(source, out var providers))
            {
                continue;
            }

            foreach (var provider in providers)
            {
                if (assigned.Add(provider))
                {
                    ordered.Add(provider);
                }
            }
        }

        if (_options.IncludeUnorderedProviders)
        {
            foreach (var provider in _providers)
            {
                if (assigned.Add(provider))
                {
                    ordered.Add(provider);
                }
            }
        }

        return ordered;
    }

    private CancellationTokenSource? CreateTimeoutToken(CancellationToken cancellationToken)
    {
        if (!_options.ResolutionTimeout.HasValue)
        {
            return null;
        }

        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_options.ResolutionTimeout.Value);
        return cts;
    }
}
