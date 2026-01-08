using System.Reflection;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using CleanArchitecture.Extensions.Multitenancy.Exceptions;
using MediatR;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.Behaviors;

/// <summary>
/// MediatR behavior that enforces tenant presence and lifecycle checks.
/// </summary>
public sealed class TenantEnforcementBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentTenant _currentTenant;
    private readonly MultitenancyOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantEnforcementBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    public TenantEnforcementBehavior(ICurrentTenant currentTenant, IOptions<MultitenancyOptions> options)
    {
        _currentTenant = currentTenant ?? throw new ArgumentNullException(nameof(currentTenant));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var requirement = ResolveRequirement(request);
        if (requirement == TenantRequirementMode.Optional)
        {
            return await next().ConfigureAwait(false);
        }

        if (!_currentTenant.IsResolved)
        {
            throw new TenantNotResolvedException();
        }

        if (!_currentTenant.IsValidated)
        {
            throw new TenantNotFoundException(_currentTenant.TenantId);
        }

        var tenant = _currentTenant.TenantInfo ?? throw new TenantNotFoundException(_currentTenant.TenantId);

        if (tenant.State == TenantState.Suspended)
        {
            throw new TenantSuspendedException(tenant.TenantId);
        }

        if (!tenant.IsActive || tenant.IsSoftDeleted || tenant.State == TenantState.Deleted || tenant.State == TenantState.PendingProvision)
        {
            throw new TenantInactiveException(tenant.TenantId);
        }

        if (tenant.ExpiresAt.HasValue && tenant.ExpiresAt.Value <= DateTimeOffset.UtcNow)
        {
            throw new TenantInactiveException(tenant.TenantId);
        }

        return await next().ConfigureAwait(false);
    }

    private TenantRequirementMode ResolveRequirement(TRequest request)
    {
        TenantRequirementMode? requirement = null;

        if (request is ITenantRequirement requirementProvider)
        {
            requirement = requirementProvider.Requirement;
        }
        else
        {
            var requestType = request.GetType();
            var attributes = requestType.GetCustomAttributes(true).OfType<ITenantRequirement>().ToList();
            if (attributes.Count > 0)
            {
                requirement = attributes.Any(attribute => attribute.Requirement == TenantRequirementMode.Required)
                    ? TenantRequirementMode.Required
                    : TenantRequirementMode.Optional;
            }
        }

        if (requirement.HasValue)
        {
            return requirement == TenantRequirementMode.Optional && !_options.AllowAnonymous
                ? TenantRequirementMode.Required
                : requirement.Value;
        }

        return _options.RequireTenantByDefault
            ? TenantRequirementMode.Required
            : TenantRequirementMode.Optional;
    }
}
