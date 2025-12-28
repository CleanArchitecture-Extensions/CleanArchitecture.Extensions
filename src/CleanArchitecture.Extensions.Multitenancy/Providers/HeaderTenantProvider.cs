using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.Providers;

/// <summary>
/// Resolves tenant identifiers from configured headers.
/// </summary>
public sealed class HeaderTenantProvider : ITenantProvider
{
    private readonly MultitenancyOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeaderTenantProvider"/> class.
    /// </summary>
    /// <param name="options">Multitenancy options.</param>
    public HeaderTenantProvider(IOptions<MultitenancyOptions> options) =>
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    /// <inheritdoc />
    public TenantResolutionSource Source => TenantResolutionSource.Header;

    /// <inheritdoc />
    public ValueTask<TenantResolutionResult> ResolveAsync(TenantResolutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var headerName in _options.HeaderNames)
        {
            if (string.IsNullOrWhiteSpace(headerName))
            {
                continue;
            }

            if (context.Headers.TryGetValue(headerName, out var value))
            {
                var candidates = TenantValueParser.Split(value);
                return ValueTask.FromResult(TenantResolutionResult.FromCandidates(candidates, Source, TenantResolutionConfidence.Medium));
            }
        }

        return ValueTask.FromResult(TenantResolutionResult.NotFound(Source));
    }
}
