# Multitenancy core: resolution pipeline

This page explains how `ITenantResolver` evaluates providers and how to customize ordering and consensus rules.

## Resolution inputs

Tenant resolution is driven by `TenantResolutionContext`, which you populate in your host adapter:

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

The core package registers these providers by default:

- `RouteTenantProvider` (uses `RouteParameterName`, high confidence)
- `HostTenantProvider` (uses `HostTenantSelector`, medium confidence)
- `HeaderTenantProvider` (uses `HeaderNames`, medium confidence)
- `QueryTenantProvider` (uses `QueryParameterName`, medium confidence)
- `ClaimTenantProvider` (uses `ClaimType`, medium confidence)
- `DefaultTenantProvider` (uses `FallbackTenant`/`FallbackTenantId`, low confidence)

Header, query, and claim providers accept multiple candidates separated by `,` or `;`. Multiple candidates are treated as ambiguous and do not resolve a tenant.

## Ordering behavior

`CompositeTenantResolutionStrategy` orders providers using `MultitenancyOptions.ResolutionOrder`:

```csharp
using CleanArchitecture.Extensions.Multitenancy.Configuration;

builder.Services.Configure<MultitenancyOptions>(options =>
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

Providers not listed in `ResolutionOrder` are still evaluated when `IncludeUnorderedProviders` is `true` (default).

## Consensus mode

When `RequireMatchAcrossSources` is enabled, the strategy collects candidates from all providers and resolves only if a single unique tenant ID exists:

```csharp
builder.Services.Configure<MultitenancyOptions>(options =>
{
    options.RequireMatchAcrossSources = true;
});
```

This mode is useful when multiple sources can be present (for example, route + header) and you want strict consistency.

## Host parsing rules

The default host selector resolves the first subdomain:

- `tenant.app.com` -> `tenant`
- `localhost` -> not resolved
- IP addresses -> not resolved

Override with a custom selector when needed:

```csharp
builder.Services.Configure<MultitenancyOptions>(options =>
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

Add custom providers for environment-specific resolution:

```csharp
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Providers;

builder.Services.AddSingleton<ITenantProvider>(
    new DelegateTenantProvider(context =>
    {
        if (context.Items.TryGetValue("tenant_override", out var value))
        {
            return value?.ToString();
        }

        return null;
    }, source: TenantResolutionSource.Custom, confidence: TenantResolutionConfidence.High));
```

## Timeouts and cancellation

Set `ResolutionTimeout` to guard against slow providers:

```csharp
builder.Services.Configure<MultitenancyOptions>(options =>
{
    options.ResolutionTimeout = TimeSpan.FromMilliseconds(50);
});
```

The timeout is linked with the request cancellation token.

## Diagnostics tips

- Inspect `TenantResolutionResult.Source`, `Confidence`, and `Candidates` in logs.
- Ambiguous candidates return `IsAmbiguous == true` and do not resolve a tenant.
- In consensus mode, a single unique candidate is required.
