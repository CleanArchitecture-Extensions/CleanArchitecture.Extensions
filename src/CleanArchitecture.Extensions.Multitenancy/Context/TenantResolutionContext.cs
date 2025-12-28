namespace CleanArchitecture.Extensions.Multitenancy;

/// <summary>
/// Provides inputs to the tenant resolution pipeline.
/// </summary>
public sealed class TenantResolutionContext
{
    /// <summary>
    /// Gets or sets the host name used for resolution.
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// Gets or sets the correlation identifier for the resolution.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets the header values used for resolution.
    /// </summary>
    public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the route values used for resolution.
    /// </summary>
    public IDictionary<string, string> RouteValues { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the query string values used for resolution.
    /// </summary>
    public IDictionary<string, string> Query { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the claims used for resolution.
    /// </summary>
    public IDictionary<string, string> Claims { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets additional resolution data.
    /// </summary>
    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>();

    /// <summary>
    /// Attempts to read a header value.
    /// </summary>
    /// <param name="name">Header name.</param>
    /// <param name="value">Resolved value.</param>
    /// <returns>True when the header exists.</returns>
    public bool TryGetHeader(string name, out string? value) => Headers.TryGetValue(name, out value);

    /// <summary>
    /// Attempts to read a route value.
    /// </summary>
    /// <param name="name">Route parameter name.</param>
    /// <param name="value">Resolved value.</param>
    /// <returns>True when the route value exists.</returns>
    public bool TryGetRouteValue(string name, out string? value) => RouteValues.TryGetValue(name, out value);

    /// <summary>
    /// Attempts to read a query string value.
    /// </summary>
    /// <param name="name">Query string name.</param>
    /// <param name="value">Resolved value.</param>
    /// <returns>True when the query value exists.</returns>
    public bool TryGetQueryValue(string name, out string? value) => Query.TryGetValue(name, out value);

    /// <summary>
    /// Attempts to read a claim value.
    /// </summary>
    /// <param name="type">Claim type.</param>
    /// <param name="value">Resolved value.</param>
    /// <returns>True when the claim exists.</returns>
    public bool TryGetClaim(string type, out string? value) => Claims.TryGetValue(type, out value);
}
