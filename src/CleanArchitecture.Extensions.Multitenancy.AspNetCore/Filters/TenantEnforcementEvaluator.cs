using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Exceptions;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Filters;

internal static class TenantEnforcementEvaluator
{
    public static Exception? Evaluate(ICurrentTenant currentTenant)
    {
        ArgumentNullException.ThrowIfNull(currentTenant);

        if (!currentTenant.IsResolved)
        {
            return new TenantNotResolvedException();
        }

        if (!currentTenant.IsValidated)
        {
            return new TenantNotFoundException(currentTenant.TenantId);
        }

        var tenant = currentTenant.TenantInfo;
        if (tenant is null)
        {
            return new TenantNotFoundException(currentTenant.TenantId);
        }

        if (tenant.State == TenantState.Suspended)
        {
            return new TenantSuspendedException(tenant.TenantId);
        }

        if (!tenant.IsActive || tenant.IsSoftDeleted || tenant.State == TenantState.Deleted || tenant.State == TenantState.PendingProvision)
        {
            return new TenantInactiveException(tenant.TenantId);
        }

        if (tenant.ExpiresAt.HasValue && tenant.ExpiresAt.Value <= DateTimeOffset.UtcNow)
        {
            return new TenantInactiveException(tenant.TenantId);
        }

        return null;
    }
}
