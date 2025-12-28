using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.Providers;

/// <summary>
/// Resolves tenant identifiers from query string values.
/// </summary>
public sealed class QueryTenantProvider : ITenantProvider
{
    private readonly MultitenancyOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryTenantProvider"/> class.
    /// </summary>
    /// <param name="options">Multitenancy options.</param>
    public QueryTenantProvider(IOptions<MultitenancyOptions> options) =>
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    /// <inheritdoc />
    public TenantResolutionSource Source => TenantResolutionSource.QueryString;

    /// <inheritdoc />
    public ValueTask<TenantResolutionResult> ResolveAsync(TenantResolutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(_options.QueryParameterName))
        {
            return ValueTask.FromResult(TenantResolutionResult.NotFound(Source));
        }

        if (context.Query.TryGetValue(_options.QueryParameterName, out var value))
        {
            var candidates = TenantValueParser.Split(value);
            return ValueTask.FromResult(TenantResolutionResult.FromCandidates(candidates, Source, TenantResolutionConfidence.Medium));
        }

        return ValueTask.FromResult(TenantResolutionResult.NotFound(Source));
    }
}
