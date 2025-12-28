using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.Context;

/// <summary>
/// Resolves tenant context using configured providers and validation policies.
/// </summary>
public sealed class TenantResolver : ITenantResolver
{
    private readonly ITenantResolutionStrategy _strategy;
    private readonly MultitenancyOptions _options;
    private readonly ITenantInfoStore? _tenantStore;
    private readonly ITenantInfoCache? _tenantCache;
    private readonly ILogger<TenantResolver> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantResolver"/> class.
    /// </summary>
    public TenantResolver(
        ITenantResolutionStrategy strategy,
        IOptions<MultitenancyOptions> options,
        ILogger<TenantResolver> logger,
        ITenantInfoStore? tenantStore = null,
        ITenantInfoCache? tenantCache = null)
    {
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantStore = tenantStore;
        _tenantCache = tenantCache;
    }

    /// <inheritdoc />
    public async Task<TenantContext?> ResolveAsync(TenantResolutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var result = await _strategy.ResolveAsync(context, cancellationToken).ConfigureAwait(false);
        if (!result.IsResolved)
        {
            _logger.LogDebug("Tenant resolution returned no match.");
            return null;
        }

        var tenantId = result.TenantId!;
        var correlationId = context.CorrelationId;

        var fallbackTenant = ResolveFallbackTenant(result);
        if (fallbackTenant is not null)
        {
            return new TenantContext(fallbackTenant, result, correlationId, isValidated: true);
        }

        var (tenantInfo, validated) = await ValidateTenantAsync(tenantId, cancellationToken).ConfigureAwait(false);
        if (tenantInfo is null)
        {
            tenantInfo = new TenantInfo(tenantId)
            {
                IsActive = false,
                State = TenantState.Unknown
            };
        }

        return new TenantContext(tenantInfo, result, correlationId, validated);
    }

    private TenantInfo? ResolveFallbackTenant(TenantResolutionResult result)
    {
        if (result.Source != TenantResolutionSource.Default)
        {
            return null;
        }

        if (_options.FallbackTenant is not null)
        {
            return TenantInfo.From(_options.FallbackTenant);
        }

        if (!string.IsNullOrWhiteSpace(_options.FallbackTenantId))
        {
            return new TenantInfo(_options.FallbackTenantId)
            {
                IsActive = true,
                State = TenantState.Active
            };
        }

        return null;
    }

    private async Task<(ITenantInfo? TenantInfo, bool Validated)> ValidateTenantAsync(string tenantId, CancellationToken cancellationToken)
    {
        switch (_options.ValidationMode)
        {
            case TenantValidationMode.None:
                return (new TenantInfo(tenantId) { IsActive = true, State = TenantState.Active }, true);
            case TenantValidationMode.Cache:
                {
                    var tenant = await ResolveFromCacheAsync(tenantId, cancellationToken).ConfigureAwait(false);
                    return (tenant, tenant is not null);
                }
            case TenantValidationMode.Repository:
                {
                    var tenant = await ResolveFromStoreAsync(tenantId, cancellationToken).ConfigureAwait(false);
                    return (tenant, tenant is not null);
                }
            default:
                return (new TenantInfo(tenantId) { IsActive = true, State = TenantState.Active }, true);
        }
    }

    private async Task<ITenantInfo?> ResolveFromCacheAsync(string tenantId, CancellationToken cancellationToken)
    {
        if (_tenantCache is null)
        {
            _logger.LogWarning("Tenant validation is set to Cache but no ITenantInfoCache is registered.");
            return null;
        }

        return await _tenantCache.GetAsync(tenantId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<ITenantInfo?> ResolveFromStoreAsync(string tenantId, CancellationToken cancellationToken)
    {
        if (_tenantStore is null)
        {
            _logger.LogWarning("Tenant validation is set to Repository but no ITenantInfoStore is registered.");
            return null;
        }

        var tenant = await _tenantStore.FindByIdAsync(tenantId, cancellationToken).ConfigureAwait(false);
        if (tenant is null)
        {
            return null;
        }

        if (_tenantCache is not null)
        {
            await _tenantCache.SetAsync(tenant, _options.ResolutionCacheTtl, cancellationToken).ConfigureAwait(false);
        }

        return tenant;
    }
}
