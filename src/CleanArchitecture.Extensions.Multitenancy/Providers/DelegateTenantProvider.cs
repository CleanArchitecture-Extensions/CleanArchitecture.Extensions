using CleanArchitecture.Extensions.Multitenancy.Abstractions;

namespace CleanArchitecture.Extensions.Multitenancy.Providers;

/// <summary>
/// Resolves tenant identifiers using a delegate.
/// </summary>
public sealed class DelegateTenantProvider : ITenantProvider
{
    private readonly Func<TenantResolutionContext, IEnumerable<string>?> _selector;
    private readonly TenantResolutionConfidence _confidence;

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateTenantProvider"/> class.
    /// </summary>
    /// <param name="selector">Delegate that returns candidate tenant identifiers.</param>
    /// <param name="source">Resolution source name.</param>
    /// <param name="confidence">Resolution confidence.</param>
    public DelegateTenantProvider(
        Func<TenantResolutionContext, IEnumerable<string>?> selector,
        TenantResolutionSource source = TenantResolutionSource.Custom,
        TenantResolutionConfidence confidence = TenantResolutionConfidence.Medium)
    {
        _selector = selector ?? throw new ArgumentNullException(nameof(selector));
        Source = source;
        _confidence = confidence;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateTenantProvider"/> class.
    /// </summary>
    /// <param name="selector">Delegate that returns a single tenant identifier.</param>
    /// <param name="source">Resolution source name.</param>
    /// <param name="confidence">Resolution confidence.</param>
    public DelegateTenantProvider(
        Func<TenantResolutionContext, string?> selector,
        TenantResolutionSource source = TenantResolutionSource.Custom,
        TenantResolutionConfidence confidence = TenantResolutionConfidence.Medium)
        : this(context =>
        {
            var value = selector(context);
            return string.IsNullOrWhiteSpace(value) ? Array.Empty<string>() : new[] { value };
        }, source, confidence)
    {
    }

    /// <inheritdoc />
    public TenantResolutionSource Source { get; }

    /// <inheritdoc />
    public ValueTask<TenantResolutionResult> ResolveAsync(TenantResolutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        var candidates = _selector(context);
        return ValueTask.FromResult(TenantResolutionResult.FromCandidates(candidates, Source, _confidence));
    }
}
