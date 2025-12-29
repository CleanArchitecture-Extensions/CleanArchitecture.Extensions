using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.ProblemDetails;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Filters;

/// <summary>
/// MVC filter that enforces tenant requirements for controller actions.
/// </summary>
public sealed class TenantEnforcementActionFilter : IAsyncActionFilter
{
    private readonly ICurrentTenant _currentTenant;
    private readonly MultitenancyOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantEnforcementActionFilter"/> class.
    /// </summary>
    /// <param name="currentTenant">Current tenant accessor.</param>
    /// <param name="options">Multitenancy options.</param>
    public TenantEnforcementActionFilter(ICurrentTenant currentTenant, IOptions<MultitenancyOptions> options)
    {
        _currentTenant = currentTenant ?? throw new ArgumentNullException(nameof(currentTenant));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var requirement = TenantRequirementResolver.Resolve(context.ActionDescriptor.EndpointMetadata, _options);
        if (requirement == TenantRequirementMode.Optional)
        {
            await next().ConfigureAwait(false);
            return;
        }

        var failure = TenantEnforcementEvaluator.Evaluate(_currentTenant);
        if (failure is not null)
        {
            if (TenantProblemDetailsMapper.TryCreate(failure, context.HttpContext, out var details))
            {
                context.Result = new ObjectResult(details)
                {
                    StatusCode = details.Status
                };
                return;
            }

            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            return;
        }

        await next().ConfigureAwait(false);
    }
}
