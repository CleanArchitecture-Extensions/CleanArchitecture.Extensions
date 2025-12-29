using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.ProblemDetails;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Filters;

/// <summary>
/// Endpoint filter that enforces tenant requirements for minimal APIs.
/// </summary>
public sealed class TenantEnforcementEndpointFilter : IEndpointFilter
{
    private readonly ICurrentTenant _currentTenant;
    private readonly MultitenancyOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantEnforcementEndpointFilter"/> class.
    /// </summary>
    /// <param name="currentTenant">Current tenant accessor.</param>
    /// <param name="options">Multitenancy options.</param>
    public TenantEnforcementEndpointFilter(ICurrentTenant currentTenant, IOptions<MultitenancyOptions> options)
    {
        _currentTenant = currentTenant ?? throw new ArgumentNullException(nameof(currentTenant));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var metadata = context.HttpContext.GetEndpoint()?.Metadata;
        var requirement = TenantRequirementResolver.Resolve(metadata, _options);
        if (requirement == TenantRequirementMode.Optional)
        {
            return await next(context).ConfigureAwait(false);
        }

        var failure = TenantEnforcementEvaluator.Evaluate(_currentTenant);
        if (failure is not null)
        {
            if (TenantProblemDetailsMapper.TryCreate(failure, context.HttpContext, out var details))
            {
                return Results.Problem(details);
            }

            return Results.Problem(statusCode: StatusCodes.Status403Forbidden, detail: failure.Message);
        }

        return await next(context).ConfigureAwait(false);
    }
}
