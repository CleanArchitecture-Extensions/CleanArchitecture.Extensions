using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.Behaviors;

/// <summary>
/// MediatR behavior that validates tenant metadata when required.
/// </summary>
public sealed class TenantValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ITenantAccessor _tenantAccessor;
    private readonly ITenantInfoStore? _tenantStore;
    private readonly ITenantInfoCache? _tenantCache;
    private readonly MultitenancyOptions _options;
    private readonly ILogger<TenantValidationBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantValidationBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    public TenantValidationBehavior(
        ITenantAccessor tenantAccessor,
        IOptions<MultitenancyOptions> options,
        ILogger<TenantValidationBehavior<TRequest, TResponse>> logger,
        ITenantInfoStore? tenantStore = null,
        ITenantInfoCache? tenantCache = null)
    {
        _tenantAccessor = tenantAccessor ?? throw new ArgumentNullException(nameof(tenantAccessor));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantStore = tenantStore;
        _tenantCache = tenantCache;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_options.ValidationMode == TenantValidationMode.None)
        {
            return await next().ConfigureAwait(false);
        }

        var context = _tenantAccessor.Current;
        if (context is null || context.IsValidated)
        {
            return await next().ConfigureAwait(false);
        }

        if (string.IsNullOrWhiteSpace(context.TenantId))
        {
            return await next().ConfigureAwait(false);
        }

        var tenant = await ResolveTenantAsync(context.TenantId, cancellationToken).ConfigureAwait(false);
        if (tenant is not null)
        {
            context.Tenant = tenant;
            context.IsValidated = true;
        }

        return await next().ConfigureAwait(false);
    }

    private async Task<ITenantInfo?> ResolveTenantAsync(string tenantId, CancellationToken cancellationToken)
    {
        switch (_options.ValidationMode)
        {
            case TenantValidationMode.Cache:
                if (_tenantCache is null)
                {
                    _logger.LogWarning("Tenant validation is set to Cache but no ITenantInfoCache is registered.");
                    return null;
                }

                return await _tenantCache.GetAsync(tenantId, cancellationToken).ConfigureAwait(false);
            case TenantValidationMode.Repository:
                if (_tenantStore is null)
                {
                    _logger.LogWarning("Tenant validation is set to Repository but no ITenantInfoStore is registered.");
                    return null;
                }

                var tenant = await _tenantStore.FindByIdAsync(tenantId, cancellationToken).ConfigureAwait(false);
                if (tenant is not null && _tenantCache is not null)
                {
                    await _tenantCache.SetAsync(tenant, _options.ResolutionCacheTtl, cancellationToken).ConfigureAwait(false);
                }

                return tenant;
            default:
                return null;
        }
    }
}
