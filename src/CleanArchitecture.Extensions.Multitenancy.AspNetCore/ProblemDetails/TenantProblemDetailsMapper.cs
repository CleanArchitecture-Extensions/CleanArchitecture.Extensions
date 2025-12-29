using CleanArchitecture.Extensions.Multitenancy.Exceptions;
using Microsoft.AspNetCore.Http;
namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.ProblemDetails;

/// <summary>
/// Maps multitenancy exceptions to HTTP problem details responses.
/// </summary>
public static class TenantProblemDetailsMapper
{
    /// <summary>
    /// Attempts to create problem details for a multitenancy exception.
    /// </summary>
    /// <param name="exception">Exception to map.</param>
    /// <param name="httpContext">HTTP context.</param>
    /// <param name="details">Problem details when mapped.</param>
    /// <returns>True when the exception was mapped.</returns>
    public static bool TryCreate(Exception exception, HttpContext httpContext, out global::Microsoft.AspNetCore.Mvc.ProblemDetails details)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(httpContext);

        switch (exception)
        {
            case TenantNotResolvedException:
                details = Create(httpContext, StatusCodes.Status400BadRequest, "Tenant not resolved", exception.Message);
                return true;
            case TenantNotFoundException:
                details = Create(httpContext, StatusCodes.Status404NotFound, "Tenant not found", exception.Message);
                return true;
            case TenantSuspendedException:
                details = Create(httpContext, StatusCodes.Status403Forbidden, "Tenant suspended", exception.Message);
                return true;
            case TenantInactiveException:
                details = Create(httpContext, StatusCodes.Status403Forbidden, "Tenant inactive", exception.Message);
                return true;
            default:
                details = new global::Microsoft.AspNetCore.Mvc.ProblemDetails();
                return false;
        }
    }

    private static global::Microsoft.AspNetCore.Mvc.ProblemDetails Create(HttpContext httpContext, int statusCode, string title, string detail)
    {
        return new global::Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
    }
}
