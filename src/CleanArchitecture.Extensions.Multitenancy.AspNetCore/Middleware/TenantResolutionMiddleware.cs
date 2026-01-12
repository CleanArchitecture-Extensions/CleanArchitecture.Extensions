using System.Diagnostics;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Context;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Options;
using CleanArchitecture.Extensions.Multitenancy.Behaviors;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantResolutionMiddleware"/> class.
    /// </summary>
    /// <param name="next">Request delegate.</param>
    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    /// <summary>
    /// Executes the middleware.
    /// </summary>
    /// <param name="httpContext">HTTP context.</param>
    /// <param name="resolver">Tenant resolver.</param>
    /// <param name="accessor">Tenant accessor.</param>
    /// <param name="contextFactory">Resolution context factory.</param>
    /// <param name="aspNetCoreOptions">ASP.NET Core multitenancy options.</param>
    /// <param name="options">Core multitenancy options.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="scopeAccessor">Correlation scope accessor.</param>
    public async Task InvokeAsync(
        HttpContext httpContext,
        ITenantResolver resolver,
        ITenantAccessor accessor,
        ITenantResolutionContextFactory contextFactory,
        IOptions<AspNetCoreMultitenancyOptions> aspNetCoreOptions,
        IOptions<MultitenancyOptions> options,
        ILogger<TenantResolutionMiddleware> logger,
        ITenantCorrelationScopeAccessor scopeAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(accessor);
        ArgumentNullException.ThrowIfNull(contextFactory);
        ArgumentNullException.ThrowIfNull(aspNetCoreOptions);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(scopeAccessor);

        var aspNetCoreOptionsValue = aspNetCoreOptions.Value ?? throw new ArgumentNullException(nameof(aspNetCoreOptions));
        var optionsValue = options.Value ?? throw new ArgumentNullException(nameof(options));

        var resolutionContext = contextFactory.Create(httpContext);
        var tenantContext = await resolver.ResolveAsync(resolutionContext, httpContext.RequestAborted).ConfigureAwait(false);

        if (aspNetCoreOptionsValue.StoreTenantInHttpContextItems)
        {
            httpContext.Items[aspNetCoreOptionsValue.HttpContextItemKey] = tenantContext;
        }

        using var tenantScope = accessor.BeginScope(tenantContext);

        IDisposable? logScope = null;
        var shouldClearScope = scopeAccessor.CurrentScope is null;
        if (shouldClearScope)
        {
            logScope = BeginLoggingScope(logger, optionsValue, tenantContext?.TenantId);
            scopeAccessor.SetScope(logScope, owned: false);
        }

        try
        {
            await _next(httpContext).ConfigureAwait(false);
        }
        finally
        {
            if (shouldClearScope)
            {
                if (ReferenceEquals(scopeAccessor.CurrentScope, logScope))
                {
                    scopeAccessor.ClearScope()?.Dispose();
                }
                else
                {
                    logScope?.Dispose();
                }
            }
        }
    }

    private static IDisposable? BeginLoggingScope(
        ILogger<TenantResolutionMiddleware> logger,
        MultitenancyOptions options,
        string? tenantId)
    {
        var scopeKey = string.IsNullOrWhiteSpace(options.LogScopeKey)
            ? "tenant_id"
            : options.LogScopeKey;

        if (options.AddTenantToActivity)
        {
            var activity = Activity.Current;
            if (activity is not null)
            {
                activity.SetBaggage(scopeKey, tenantId ?? string.Empty);
                activity.SetTag(scopeKey, tenantId);
            }
        }

        if (!options.AddTenantToLogScope)
        {
            return null;
        }

        return logger.BeginScope(new Dictionary<string, object?> { [scopeKey] = tenantId });
    }
}
