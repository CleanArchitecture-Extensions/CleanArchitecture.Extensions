namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Attributes;

/// <summary>
/// Describes the tenant route parameter used by an endpoint.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
public sealed class TenantRouteAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TenantRouteAttribute"/> class.
    /// </summary>
    /// <param name="routeParameterName">Route parameter name.</param>
    public TenantRouteAttribute(string routeParameterName)
    {
        if (string.IsNullOrWhiteSpace(routeParameterName))
        {
            throw new ArgumentException("Route parameter name cannot be empty.", nameof(routeParameterName));
        }

        RouteParameterName = routeParameterName;
    }

    /// <summary>
    /// Gets the route parameter name.
    /// </summary>
    public string RouteParameterName { get; }
}
