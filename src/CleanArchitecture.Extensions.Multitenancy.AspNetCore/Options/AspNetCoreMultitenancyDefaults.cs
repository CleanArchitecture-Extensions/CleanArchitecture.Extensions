namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Options;

/// <summary>
/// Defaults for the ASP.NET Core multitenancy adapter.
/// </summary>
public static class AspNetCoreMultitenancyDefaults
{
    /// <summary>
    /// Default HttpContext.Items key for resolved tenant context.
    /// </summary>
    public const string TenantContextItemKey = "CleanArchitecture.Extensions.Multitenancy.TenantContext";
}
