# Configuration Reference

Configuration keys and environment variables for extensions.

- Use the Options pattern to bind configuration sections to extension options.
- Section names are not enforced; the names below are recommended for consistency.

## Available references

- Multitenancy options: [multitenancy-options.md](multitenancy-options.md)

## Recommended sections

- `Extensions:Caching` -> `CachingOptions`
- `Extensions:Caching:QueryBehavior` -> `QueryCachingBehaviorOptions`
- `Extensions:Multitenancy` -> `MultitenancyOptions`
- `Extensions:Multitenancy:AspNetCore` -> `AspNetCoreMultitenancyOptions`
- `Extensions:Multitenancy:EFCore` -> `EfCoreMultitenancyOptions`

## Example

```json
{
  "Extensions": {
    "Caching": {
      "Enabled": true,
      "DefaultNamespace": "MyApp",
      "QueryBehavior": {
        "DefaultTtl": "00:05:00",
        "CacheNullValues": false
      }
    },
    "Multitenancy": {
      "RequireTenantByDefault": true,
      "HeaderNames": [ "X-Tenant-ID" ],
      "EFCore": {
        "Mode": "SharedDatabase",
        "TenantIdPropertyName": "TenantId"
      }
    }
  }
}
```

Bind sections in your host:

```csharp
builder.Services.Configure<CachingOptions>(
    builder.Configuration.GetSection("Extensions:Caching"));
builder.Services.Configure<QueryCachingBehaviorOptions>(
    builder.Configuration.GetSection("Extensions:Caching:QueryBehavior"));
builder.Services.Configure<MultitenancyOptions>(
    builder.Configuration.GetSection("Extensions:Multitenancy"));
builder.Services.Configure<EfCoreMultitenancyOptions>(
    builder.Configuration.GetSection("Extensions:Multitenancy:EFCore"));
```
