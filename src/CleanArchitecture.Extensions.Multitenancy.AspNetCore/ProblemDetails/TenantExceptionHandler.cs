using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.ProblemDetails;

/// <summary>
/// Exception handler that maps multitenancy exceptions to ProblemDetails responses.
/// </summary>
public sealed class TenantExceptionHandler : IExceptionHandler
{
    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        if (!TenantProblemDetailsMapper.TryCreate(exception, httpContext, out var details))
        {
            return false;
        }

        await Results.Problem(details).ExecuteAsync(httpContext).ConfigureAwait(false);
        return true;
    }
}
