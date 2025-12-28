using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.Providers;

/// <summary>
/// Provides a fallback tenant when no other provider resolves a tenant.
/// </summary>
public sealed class DefaultTenantProvider : ITenantProvider
{
    private readonly MultitenancyOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultTenantProvider"/> class.
    /// </summary>
    /// <param name="options">Multitenancy options.</param>
    public DefaultTenantProvider(IOptions<MultitenancyOptions> options) =>
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    /// <inheritdoc />
    public TenantResolutionSource Source => TenantResolutionSource.Default;

    /// <inheritdoc />
    public ValueTask<TenantResolutionResult> ResolveAsync(TenantResolutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        var tenantId = _options.FallbackTenant?.TenantId ?? _options.FallbackTenantId;

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return ValueTask.FromResult(TenantResolutionResult.NotFound(Source));
        }

        return ValueTask.FromResult(TenantResolutionResult.Resolved(tenantId, Source, TenantResolutionConfidence.Low));
    }
}
