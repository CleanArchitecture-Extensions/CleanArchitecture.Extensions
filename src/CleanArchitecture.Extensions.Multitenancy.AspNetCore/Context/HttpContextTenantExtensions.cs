using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Options;
using Microsoft.AspNetCore.Http;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Context;

/// <summary>
/// HTTP context helpers for multitenancy.
/// </summary>
public static class HttpContextTenantExtensions
{
    /// <summary>
    /// Gets the tenant context stored in <see cref="HttpContext.Items"/>.
    /// </summary>
    /// <param name="httpContext">HTTP context.</param>
    /// <param name="itemKey">Optional item key override.</param>
    public static TenantContext? GetTenantContext(this HttpContext httpContext, string? itemKey = null)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var key = string.IsNullOrWhiteSpace(itemKey)
            ? AspNetCoreMultitenancyDefaults.TenantContextItemKey
            : itemKey;

        return httpContext.Items.TryGetValue(key, out var value)
            ? value as TenantContext
            : null;
    }
}
