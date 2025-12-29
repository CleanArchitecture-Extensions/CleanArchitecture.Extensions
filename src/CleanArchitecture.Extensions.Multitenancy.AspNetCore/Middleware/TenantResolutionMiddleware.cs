using System.Diagnostics;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Context;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Options;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Middleware;

/// <summary>
/// Resolves tenant context for HTTP requests and stores it in the current tenant accessor.
/// </summary>
public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITenantResolver _resolver;
    private readonly ITenantAccessor _accessor;
    private readonly ITenantResolutionContextFactory _contextFactory;
    private readonly AspNetCoreMultitenancyOptions _aspNetCoreOptions;
    private readonly MultitenancyOptions _options;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantResolutionMiddleware"/> class.
    /// </summary>
    /// <param name="next">Request delegate.</param>
    /// <param name="resolver">Tenant resolver.</param>
    /// <param name="accessor">Tenant accessor.</param>
    /// <param name="contextFactory">Resolution context factory.</param>
    /// <param name="aspNetCoreOptions">ASP.NET Core multitenancy options.</param>
    /// <param name="options">Core multitenancy options.</param>
    /// <param name="logger">Logger.</param>
    public TenantResolutionMiddleware(
        RequestDelegate next,
        ITenantResolver resolver,
        ITenantAccessor accessor,
        ITenantResolutionContextFactory contextFactory,
        IOptions<AspNetCoreMultitenancyOptions> aspNetCoreOptions,
        IOptions<MultitenancyOptions> options,
        ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _aspNetCoreOptions = aspNetCoreOptions?.Value ?? throw new ArgumentNullException(nameof(aspNetCoreOptions));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the middleware.
    /// </summary>
    /// <param name="httpContext">HTTP context.</param>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var resolutionContext = _contextFactory.Create(httpContext);
        var tenantContext = await _resolver.ResolveAsync(resolutionContext, httpContext.RequestAborted).ConfigureAwait(false);

        if (_aspNetCoreOptions.StoreTenantInHttpContextItems)
        {
            httpContext.Items[_aspNetCoreOptions.HttpContextItemKey] = tenantContext;
        }

        using var tenantScope = _accessor.BeginScope(tenantContext);
        using var logScope = BeginLoggingScope(tenantContext?.TenantId);

        await _next(httpContext).ConfigureAwait(false);
    }

    private IDisposable? BeginLoggingScope(string? tenantId)
    {
        if (!_options.AddTenantToLogScope)
        {
            return null;
        }

        var scopeKey = string.IsNullOrWhiteSpace(_options.LogScopeKey)
            ? "tenant_id"
            : _options.LogScopeKey;

        var scope = _logger.BeginScope(new Dictionary<string, object?> { [scopeKey] = tenantId });

        if (_options.AddTenantToActivity)
        {
            var activity = Activity.Current;
            if (activity is not null)
            {
                activity.SetBaggage(scopeKey, tenantId ?? string.Empty);
                activity.SetTag(scopeKey, tenantId);
            }
        }

        return scope;
    }
}
