# Reference: Multitenancy options

`MultitenancyOptions` configures tenant resolution, validation, and correlation.

## Options

| Option | Type | Default | Notes |
| --- | --- | --- | --- |
| `RequireTenantByDefault` | `bool` | `true` | Require a tenant unless overridden by a requirement. |
| `AllowAnonymous` | `bool` | `false` | Allow tenant-less requests when optional. |
| `FallbackTenant` | `TenantInfo?` | `null` | Full fallback tenant model. |
| `FallbackTenantId` | `string?` | `null` | Fallback tenant ID when no tenant is resolved. |
| `ResolutionOrder` | `List<TenantResolutionSource>` | Route, Host, Header, QueryString, Claim, Default | Provider evaluation order. |
| `HeaderNames` | `string[]` | `["X-Tenant-ID"]` | Header names to inspect for tenant IDs. |
| `ClaimType` | `string` | `"tenant_id"` | Claim type for tenant ID. |
| `RouteParameterName` | `string` | `"tenantId"` | Route parameter name. |
| `QueryParameterName` | `string` | `"tenantId"` | Query parameter name. |
| `ResolutionCacheTtl` | `TimeSpan?` | `00:05:00` | TTL for tenant cache entries. |
| `ResolutionTimeout` | `TimeSpan?` | `null` | Timeout for resolution pipeline. |
| `ValidationMode` | `TenantValidationMode` | `None` | Validation strategy. |
| `RequireMatchAcrossSources` | `bool` | `false` | Require a single tenant match across all sources. |
| `IncludeUnorderedProviders` | `bool` | `true` | Evaluate providers not listed in `ResolutionOrder`. |
| `AddTenantToLogScope` | `bool` | `true` | Add tenant ID to log scope. |
| `LogScopeKey` | `string` | `"tenant_id"` | Log scope key name. |
| `AddTenantToActivity` | `bool` | `true` | Add tenant ID to activity tags/baggage. |
| `HostTenantSelector` | `Func<string,string?>?` | `null` | Custom host-to-tenant selector. |

## TenantValidationMode

- `None`: no validation; tenant is assumed active.
- `Cache`: validate using `ITenantInfoCache` only.
- `Repository`: validate using `ITenantInfoStore` (and cache if configured).

## TenantResolutionSource

`TenantResolutionSource` values include `Route`, `Host`, `Header`, `QueryString`, `Claim`, `Default`, `Custom`, and `Composite`.

## Example configuration

```csharp
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Configuration;

services.Configure<MultitenancyOptions>(options =>
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
