using Microsoft.AspNetCore.Http;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Context;

/// <summary>
/// Creates <see cref="TenantResolutionContext"/> instances from HTTP requests.
/// </summary>
public interface ITenantResolutionContextFactory
{
    /// <summary>
    /// Creates a tenant resolution context from the current HTTP request.
    /// </summary>
    /// <param name="httpContext">HTTP context.</param>
    /// <returns>Resolution context.</returns>
    TenantResolutionContext Create(HttpContext httpContext);
}
