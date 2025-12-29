using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Context;

/// <summary>
/// Creates tenant resolution contexts from ASP.NET Core requests.
/// </summary>
public sealed class DefaultTenantResolutionContextFactory : ITenantResolutionContextFactory
{
    private readonly AspNetCoreMultitenancyOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultTenantResolutionContextFactory"/> class.
    /// </summary>
    /// <param name="options">ASP.NET Core multitenancy options.</param>
    public DefaultTenantResolutionContextFactory(IOptions<AspNetCoreMultitenancyOptions> options) =>
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    /// <inheritdoc />
    public TenantResolutionContext Create(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var context = new TenantResolutionContext
        {
            Host = httpContext.Request.Host.Host,
            CorrelationId = ResolveCorrelationId(httpContext)
        };

        foreach (var header in httpContext.Request.Headers)
        {
            context.Headers[header.Key] = header.Value.ToString();
        }

        foreach (var route in httpContext.Request.RouteValues)
        {
            if (route.Value is not null)
            {
                context.RouteValues[route.Key] = route.Value.ToString()!;
            }
        }

        foreach (var query in httpContext.Request.Query)
        {
            context.Query[query.Key] = query.Value.ToString();
        }

        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            foreach (var claim in httpContext.User.Claims)
            {
                if (!context.Claims.ContainsKey(claim.Type))
                {
                    context.Claims[claim.Type] = claim.Value;
                }
            }
        }

        return context;
    }

    private string? ResolveCorrelationId(HttpContext httpContext)
    {
        if (!string.IsNullOrWhiteSpace(_options.CorrelationIdHeaderName)
            && httpContext.Request.Headers.TryGetValue(_options.CorrelationIdHeaderName, out var headerValue))
        {
            var header = headerValue.ToString();
            if (!string.IsNullOrWhiteSpace(header))
            {
                return header;
            }
        }

        return _options.UseTraceIdentifierAsCorrelationId ? httpContext.TraceIdentifier : null;
    }
}
