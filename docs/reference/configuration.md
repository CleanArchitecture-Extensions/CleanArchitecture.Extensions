# Configuration reference

This page explains how to configure extensions using the Options pattern.

## Recommended configuration sections

| Section                              | Options type                    | Notes                            |
| ------------------------------------ | ------------------------------- | -------------------------------- |
| `Extensions:Caching`                 | `CachingOptions`                | Core caching defaults.           |
| `Extensions:Caching:QueryBehavior`   | `QueryCachingBehaviorOptions`   | Query caching behavior settings. |
| `Extensions:Multitenancy`            | `MultitenancyOptions`           | Core multitenancy settings.      |
| `Extensions:Multitenancy:AspNetCore` | `AspNetCoreMultitenancyOptions` | ASP.NET Core adapter settings.   |
| `Extensions:Multitenancy:EFCore`     | `EfCoreMultitenancyOptions`     | EF Core adapter settings.        |

## Example configuration

```json
{
  "Extensions": {
    "Caching": {
      "Enabled": true,
      "DefaultNamespace": "MyApp",
      "MaxEntrySizeBytes": 262144,
      "QueryBehavior": {
        "DefaultTtl": "00:05:00",
        "CacheNullValues": false
      }
    },
    "Multitenancy": {
      "RequireTenantByDefault": true,
      "HeaderNames": ["X-Tenant-ID"],
      "RouteParameterName": "tenantId",
      "ValidationMode": "Repository",
      "AspNetCore": {
        "CorrelationIdHeaderName": "X-Correlation-ID",
        "StoreTenantInHttpContextItems": true
      },
      "EFCore": {
        "Mode": "SharedDatabase",
        "TenantIdPropertyName": "TenantId",
        "UseShadowTenantId": true
      }
    }
  }
}
```

## Bind configuration in code

```csharp
builder.Services.Configure<CachingOptions>(
    builder.Configuration.GetSection("Extensions:Caching"));

builder.Services.Configure<QueryCachingBehaviorOptions>(
    builder.Configuration.GetSection("Extensions:Caching:QueryBehavior"));

builder.Services.Configure<MultitenancyOptions>(
    builder.Configuration.GetSection("Extensions:Multitenancy"));

builder.Services.Configure<AspNetCoreMultitenancyOptions>(
    builder.Configuration.GetSection("Extensions:Multitenancy:AspNetCore"));

builder.Services.Configure<EfCoreMultitenancyOptions>(
    builder.Configuration.GetSection("Extensions:Multitenancy:EFCore"));
```

## Options that must be set in code

Some options are delegates (for example, `HostTenantSelector`, `SchemaNameProvider`, `ConnectionStringProvider`). These cannot be bound from JSON and must be configured in code.

## See also

- [Caching options](caching-options.md)
- [Multitenancy options](multitenancy-options.md)
- [AspNetCore multitenancy options](aspnetcore-multitenancy-options.md)
- [EF Core multitenancy options](efcore-multitenancy-options.md)
