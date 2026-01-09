namespace CleanArchitecture.Extensions.Multitenancy.Configuration;

/// <summary>
/// Configures multitenancy defaults for the extensions package.
/// </summary>
public sealed class MultitenancyOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether tenants are required by default.
    /// </summary>
    public bool RequireTenantByDefault { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether tenant-less requests are allowed when explicitly marked optional.
    /// When false, optional requirements are treated as required.
    /// </summary>
    public bool AllowAnonymous { get; set; } = true;

    /// <summary>
    /// Gets or sets a fallback tenant used when no tenant is resolved.
    /// </summary>
    public TenantInfo? FallbackTenant { get; set; }

    /// <summary>
    /// Gets or sets a fallback tenant identifier used when no tenant is resolved.
    /// </summary>
    public string? FallbackTenantId { get; set; }

    /// <summary>
    /// Gets or sets the ordered list of resolution sources.
    /// </summary>
    public List<TenantResolutionSource> ResolutionOrder { get; set; } = new()
    {
        TenantResolutionSource.Route,
        TenantResolutionSource.Host,
        TenantResolutionSource.Header,
        TenantResolutionSource.QueryString,
        TenantResolutionSource.Claim,
        TenantResolutionSource.Default
    };

    /// <summary>
    /// Gets or sets the header names to inspect for tenant identifiers.
    /// </summary>
    public string[] HeaderNames { get; set; } = { "X-Tenant-ID" };

    /// <summary>
    /// Gets or sets the claim type that stores the tenant identifier.
    /// </summary>
    public string ClaimType { get; set; } = "tenant_id";

    /// <summary>
    /// Gets or sets the route parameter name for tenant identifiers.
    /// </summary>
    public string RouteParameterName { get; set; } = "tenantId";

    /// <summary>
    /// Gets or sets the query string parameter name for tenant identifiers.
    /// </summary>
    public string QueryParameterName { get; set; } = "tenantId";

    /// <summary>
    /// Gets or sets the cache TTL for resolved tenant metadata.
    /// </summary>
    public TimeSpan? ResolutionCacheTtl { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the timeout applied to the resolution pipeline.
    /// </summary>
    public TimeSpan? ResolutionTimeout { get; set; }

    /// <summary>
    /// Gets or sets the tenant validation mode.
    /// </summary>
    public TenantValidationMode ValidationMode { get; set; } = TenantValidationMode.None;

    /// <summary>
    /// Gets or sets a value indicating whether multiple sources must agree on the tenant identifier.
    /// </summary>
    public bool RequireMatchAcrossSources { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether providers not listed in <see cref="ResolutionOrder"/> are evaluated.
    /// </summary>
    public bool IncludeUnorderedProviders { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the tenant identifier should be added to log scopes.
    /// </summary>
    public bool AddTenantToLogScope { get; set; } = true;

    /// <summary>
    /// Gets or sets the log scope key used for tenant identifiers.
    /// </summary>
    public string LogScopeKey { get; set; } = "tenant_id";

    /// <summary>
    /// Gets or sets a value indicating whether the tenant identifier should be added to activity baggage.
    /// </summary>
    public bool AddTenantToActivity { get; set; } = true;

    /// <summary>
    /// Gets or sets the host-to-tenant resolver callback.
    /// </summary>
    public Func<string, string?>? HostTenantSelector { get; set; }

    /// <summary>
    /// Gets the default options instance.
    /// </summary>
    public static MultitenancyOptions Default => new();
}
