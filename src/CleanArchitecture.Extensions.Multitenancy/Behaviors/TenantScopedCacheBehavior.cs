using CleanArchitecture.Extensions.Caching.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Extensions.Multitenancy.Behaviors;

/// <summary>
/// MediatR behavior that ensures cache scopes align with the current tenant.
/// </summary>
public sealed class TenantScopedCacheBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentTenant _currentTenant;
    private readonly ICacheScope _cacheScope;
    private readonly ILogger<TenantScopedCacheBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantScopedCacheBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    public TenantScopedCacheBehavior(
        ICurrentTenant currentTenant,
        ICacheScope cacheScope,
        ILogger<TenantScopedCacheBehavior<TRequest, TResponse>> logger)
    {
        _currentTenant = currentTenant ?? throw new ArgumentNullException(nameof(currentTenant));
        _cacheScope = cacheScope ?? throw new ArgumentNullException(nameof(cacheScope));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tenantId = _currentTenant.TenantId;
        if (!string.IsNullOrWhiteSpace(tenantId) && !string.Equals(_cacheScope.TenantId, tenantId, StringComparison.Ordinal))
        {
            _logger.LogWarning(
                "Cache scope tenant mismatch. Expected {TenantId}, got {ScopeTenantId}.",
                tenantId,
                _cacheScope.TenantId);
        }

        return await next().ConfigureAwait(false);
    }
}
