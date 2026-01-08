using System.Diagnostics;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.Behaviors;

/// <summary>
/// MediatR pre-processor that enriches logging and activity scopes with tenant identifiers.
/// </summary>
public sealed class TenantCorrelationPreProcessor<TRequest> : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    private readonly ICurrentTenant _currentTenant;
    private readonly MultitenancyOptions _options;
    private readonly ILogger<TenantCorrelationPreProcessor<TRequest>> _logger;
    private readonly ITenantCorrelationScopeAccessor _scopeAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantCorrelationPreProcessor{TRequest}"/> class.
    /// </summary>
    public TenantCorrelationPreProcessor(
        ICurrentTenant currentTenant,
        IOptions<MultitenancyOptions> options,
        ILogger<TenantCorrelationPreProcessor<TRequest>> logger,
        ITenantCorrelationScopeAccessor scopeAccessor)
    {
        _currentTenant = currentTenant ?? throw new ArgumentNullException(nameof(currentTenant));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scopeAccessor = scopeAccessor ?? throw new ArgumentNullException(nameof(scopeAccessor));
    }

    /// <inheritdoc />
    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_scopeAccessor.CurrentScope is not null)
        {
            return Task.CompletedTask;
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
            return Task.CompletedTask;
        }

        var scope = _logger.BeginScope(new Dictionary<string, object?> { [scopeKey] = tenantId });
        _scopeAccessor.SetScope(scope, owned: true);

        return Task.CompletedTask;
    }
}
