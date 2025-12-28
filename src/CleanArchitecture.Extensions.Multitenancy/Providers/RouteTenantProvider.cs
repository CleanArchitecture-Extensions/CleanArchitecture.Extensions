using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.Providers;

/// <summary>
/// Resolves tenant identifiers from route values.
/// </summary>
public sealed class RouteTenantProvider : ITenantProvider
{
    private readonly MultitenancyOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="RouteTenantProvider"/> class.
    /// </summary>
    /// <param name="options">Multitenancy options.</param>
    public RouteTenantProvider(IOptions<MultitenancyOptions> options) =>
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    /// <inheritdoc />
    public TenantResolutionSource Source => TenantResolutionSource.Route;

    /// <inheritdoc />
    public ValueTask<TenantResolutionResult> ResolveAsync(TenantResolutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(_options.RouteParameterName))
        {
            return ValueTask.FromResult(TenantResolutionResult.NotFound(Source));
        }

        if (context.RouteValues.TryGetValue(_options.RouteParameterName, out var value))
        {
            var candidates = TenantValueParser.Split(value);
            return ValueTask.FromResult(TenantResolutionResult.FromCandidates(candidates, Source, TenantResolutionConfidence.High));
        }

        return ValueTask.FromResult(TenantResolutionResult.NotFound(Source));
    }
}
