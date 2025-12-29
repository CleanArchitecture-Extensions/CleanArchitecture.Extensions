using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Configuration;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Filters;

internal static class TenantRequirementResolver
{
    public static TenantRequirementMode Resolve(IEnumerable<object>? metadata, MultitenancyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (metadata is not null)
        {
            var requirements = metadata.OfType<ITenantRequirement>().ToList();
            if (requirements.Count > 0)
            {
                return requirements.Any(requirement => requirement.Requirement == TenantRequirementMode.Required)
                    ? TenantRequirementMode.Required
                    : TenantRequirementMode.Optional;
            }
        }

        return options.RequireTenantByDefault && !options.AllowAnonymous
            ? TenantRequirementMode.Required
            : TenantRequirementMode.Optional;
    }
}
