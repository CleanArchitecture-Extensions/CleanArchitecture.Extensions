using Microsoft.AspNetCore.Builder;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Middleware;

/// <summary>
/// Application builder helpers for multitenancy.
/// </summary>
public static class ApplicationBuilderExtensions
{
    private const string MiddlewareRegisteredKey = "CleanArchitecture.Extensions.Multitenancy.MiddlewareRegistered";

    /// <summary>
    /// Adds the multitenancy resolution middleware to the pipeline.
    /// </summary>
    /// <param name="app">Application builder.</param>
    public static IApplicationBuilder UseCleanArchitectureMultitenancy(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (app.Properties.ContainsKey(MiddlewareRegisteredKey))
        {
            return app;
        }

        app.Properties[MiddlewareRegisteredKey] = true;
        return app.UseMiddleware<TenantResolutionMiddleware>();
    }
}
