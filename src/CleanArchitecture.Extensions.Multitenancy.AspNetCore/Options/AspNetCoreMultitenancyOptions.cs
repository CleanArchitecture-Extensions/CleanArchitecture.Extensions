using Microsoft.AspNetCore.Http;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Options;

/// <summary>
/// Configures ASP.NET Core-specific multitenancy behavior.
/// </summary>
public sealed class AspNetCoreMultitenancyOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the resolved tenant is stored in <see cref="HttpContext.Items"/>.
    /// </summary>
    public bool StoreTenantInHttpContextItems { get; set; } = true;

    /// <summary>
    /// Gets or sets the <see cref="HttpContext.Items"/> key used to store the tenant context.
    /// </summary>
    public string HttpContextItemKey { get; set; } = AspNetCoreMultitenancyDefaults.TenantContextItemKey;

    /// <summary>
    /// Gets or sets the header name used to override the correlation identifier.
    /// </summary>
    public string? CorrelationIdHeaderName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="HttpContext.TraceIdentifier"/> is used as a fallback correlation ID.
    /// </summary>
    public bool UseTraceIdentifierAsCorrelationId { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether OpenAPI/ApiExplorer integration is enabled.
    /// </summary>
    public bool EnableOpenApiIntegration { get; set; } = true;

    /// <summary>
    /// Gets the default options instance.
    /// </summary>
    public static AspNetCoreMultitenancyOptions Default => new();
}
