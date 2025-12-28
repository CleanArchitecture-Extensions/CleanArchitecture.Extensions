# Multitenancy Core: Resolution pipeline

This page explains how tenant resolution works and how to customize provider order and consensus rules.

## Resolution inputs

Tenant resolution is driven by `TenantResolutionContext`, which is populated by your host:

```csharp
using CleanArchitecture.Extensions.Multitenancy;

var context = new TenantResolutionContext
{
    Host = httpContext.Request.Host.Host,
    CorrelationId = httpContext.TraceIdentifier
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

foreach (var claim in httpContext.User.Claims)
{
    context.Claims[claim.Type] = claim.Value;
}
```

## Built-in providers

The core package registers the following providers by default:

- `RouteTenantProvider` - uses `RouteParameterName` and returns high confidence.
- `HostTenantProvider` - uses `HostTenantSelector` (defaults to first subdomain).
- `HeaderTenantProvider` - scans `HeaderNames`.
- `QueryTenantProvider` - uses `QueryParameterName`.
- `ClaimTenantProvider` - uses `ClaimType`.
- `DefaultTenantProvider` - uses `FallbackTenant` / `FallbackTenantId`.

Header/query/claim values can contain multiple candidates separated by `,` or `;`. Multiple candidates are treated as ambiguous and will not resolve a tenant.

## Ordering and consensus

Resolution order is driven by `MultitenancyOptions.ResolutionOrder`:

```csharp
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Configuration;

services.Configure<MultitenancyOptions>(options =>
{
    options.ResolutionOrder = new List<TenantResolutionSource>
    {
        TenantResolutionSource.Route,
        TenantResolutionSource.Host,
        TenantResolutionSource.Header,
        TenantResolutionSource.QueryString,
        TenantResolutionSource.Claim,
        TenantResolutionSource.Default
    };
});
```

If `RequireMatchAcrossSources` is enabled, the strategy collects candidates from all providers and resolves only when there is exactly one unique candidate:

```csharp
services.Configure<MultitenancyOptions>(options =>
{
    options.RequireMatchAcrossSources = true;
});
```

## Host parsing rules

By default, the host provider uses the first subdomain:

- `tenant.app.com` -> `tenant`
- `localhost` -> not resolved
- IP addresses -> not resolved

Override with a custom selector when needed:

```csharp
services.Configure<MultitenancyOptions>(options =>
{
    options.HostTenantSelector = host =>
    {
        if (host.EndsWith(".internal", StringComparison.OrdinalIgnoreCase))
        {
            return "internal";
        }

        return host.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();
    };
});
```

## Custom providers

You can add custom providers for environment-specific resolution:

```csharp
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Providers;

services.AddSingleton<ITenantProvider>(
    new DelegateTenantProvider(context =>
    {
        if (context.Items.TryGetValue("tenant_override", out var value))
        {
            return value?.ToString();
        }

        return null;
    }, source: TenantResolutionSource.Custom, confidence: TenantResolutionConfidence.High));
```

Custom providers not listed in `ResolutionOrder` are still evaluated when `IncludeUnorderedProviders` is true (default).

## Timeouts and cancellation

Set `ResolutionTimeout` to guard against slow providers:

```csharp
services.Configure<MultitenancyOptions>(options =>
{
    options.ResolutionTimeout = TimeSpan.FromMilliseconds(50);
});
```

The composite strategy links this timeout with the request cancellation token.

## Diagnostics tips

- Log `TenantResolutionResult.Source`, `Confidence`, and `Candidates` for debugging.
- Ambiguous candidates result in no resolved tenant; tighten your sources or enable consensus.
