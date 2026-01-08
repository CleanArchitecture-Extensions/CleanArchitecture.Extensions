using System.Diagnostics;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.Behaviors;

/// <summary>
/// MediatR behavior that enriches logging and activity scopes with tenant identifiers.
/// </summary>
public sealed class TenantCorrelationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentTenant _currentTenant;
    private readonly MultitenancyOptions _options;
    private readonly ILogger<TenantCorrelationBehavior<TRequest, TResponse>> _logger;
    private readonly ITenantCorrelationScopeAccessor _scopeAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantCorrelationBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    public TenantCorrelationBehavior(
        ICurrentTenant currentTenant,
        IOptions<MultitenancyOptions> options,
        ILogger<TenantCorrelationBehavior<TRequest, TResponse>> logger,
        ITenantCorrelationScopeAccessor scopeAccessor)
    {
        _currentTenant = currentTenant ?? throw new ArgumentNullException(nameof(currentTenant));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scopeAccessor = scopeAccessor ?? throw new ArgumentNullException(nameof(scopeAccessor));
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existingScope = _scopeAccessor.CurrentScope;
        if (existingScope is not null)
        {
            return await next().ConfigureAwait(false);
        }

        var tenantId = _currentTenant.TenantId;
        var scopeKey = string.IsNullOrWhiteSpace(_options.LogScopeKey) ? "tenant_id" : _options.LogScopeKey;
        var activity = Activity.Current;

        if (_options.AddTenantToActivity && activity is not null)
        {
            activity.SetBaggage(scopeKey, tenantId ?? string.Empty);
            activity.SetTag(scopeKey, tenantId);
        }

        if (!_options.AddTenantToLogScope)
        {
            return await next().ConfigureAwait(false);
        }

        var scope = _logger.BeginScope(new Dictionary<string, object?> { [scopeKey] = tenantId });
        _scopeAccessor.SetScope(scope, owned: true);

        try
        {
            return await next().ConfigureAwait(false);
        }
        finally
        {
            _scopeAccessor.ClearScope(onlyIfOwned: true)?.Dispose();
        }
    }
}
