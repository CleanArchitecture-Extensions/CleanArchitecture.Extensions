# Reference: Multitenancy options

`MultitenancyOptions` configures tenant resolution, validation, and correlation.

## Options

| Option | Type | Default | Description |
| --- | --- | --- | --- |
| `RequireTenantByDefault` | `bool` | `true` | Require a tenant unless a request marks itself optional. |
| `AllowAnonymous` | `bool` | `false` | Allow tenant-less requests when optional. |
| `FallbackTenant` | `TenantInfo?` | `null` | Full fallback tenant model (set in code). |
| `FallbackTenantId` | `string?` | `null` | Fallback tenant ID when no tenant is resolved. |
| `ResolutionOrder` | `List<TenantResolutionSource>` | Route, Host, Header, QueryString, Claim, Default | Provider evaluation order. |
| `HeaderNames` | `string[]` | `["X-Tenant-ID"]` | Header names inspected for tenant IDs. |
| `ClaimType` | `string` | `"tenant_id"` | Claim type for tenant ID. |
| `RouteParameterName` | `string` | `"tenantId"` | Route parameter name. |
| `QueryParameterName` | `string` | `"tenantId"` | Query parameter name. |
| `ResolutionCacheTtl` | `TimeSpan?` | `00:05:00` | TTL for tenant cache entries when validation uses a cache. |
| `ResolutionTimeout` | `TimeSpan?` | `null` | Timeout for the resolution pipeline. |
| `ValidationMode` | `TenantValidationMode` | `None` | Validation strategy. |
| `RequireMatchAcrossSources` | `bool` | `false` | Require a single unique tenant ID across all sources. |
| `IncludeUnorderedProviders` | `bool` | `true` | Evaluate providers not listed in `ResolutionOrder`. |
| `AddTenantToLogScope` | `bool` | `true` | Add tenant ID to log scopes. |
| `LogScopeKey` | `string` | `"tenant_id"` | Log scope key name. |
| `AddTenantToActivity` | `bool` | `true` | Add tenant ID to activity tags/baggage. |
| `HostTenantSelector` | `Func<string,string?>?` | `null` | Custom host-to-tenant selector (set in code). |

## TenantValidationMode

- `None`: no validation; tenant is assumed active.
- `Cache`: validate using `ITenantInfoCache` only.
- `Repository`: validate using `ITenantInfoStore` and optionally cache results.

## TenantResolutionSource

Values include `Route`, `Host`, `Header`, `QueryString`, `Claim`, `Default`, `Custom`, and `Composite`.

## Example configuration

```csharp
builder.Services.Configure<MultitenancyOptions>(options =>
{
    options.RequireTenantByDefault = true;
    options.AllowAnonymous = false;
    options.ValidationMode = TenantValidationMode.Repository;
    options.ResolutionCacheTtl = TimeSpan.FromMinutes(5);
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
