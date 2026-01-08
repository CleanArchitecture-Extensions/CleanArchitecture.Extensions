using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Configuration;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Filters;

internal static class TenantRequirementResolver
{
    public static TenantRequirementMode Resolve(IEnumerable<object>? metadata, MultitenancyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        TenantRequirementMode? requirement = null;

        if (metadata is not null)
        {
            var requirements = metadata.OfType<ITenantRequirement>().ToList();
            if (requirements.Count > 0)
            {
                requirement = requirements.Any(item => item.Requirement == TenantRequirementMode.Required)
                    ? TenantRequirementMode.Required
                    : TenantRequirementMode.Optional;
            }
        }

        if (requirement.HasValue)
        {
            return requirement == TenantRequirementMode.Optional && !options.AllowAnonymous
                ? TenantRequirementMode.Required
                : requirement.Value;
        }

        return options.RequireTenantByDefault
            ? TenantRequirementMode.Required
            : TenantRequirementMode.Optional;
    }
}
