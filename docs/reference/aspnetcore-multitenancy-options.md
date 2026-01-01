# Reference: AspNetCore multitenancy options

`AspNetCoreMultitenancyOptions` configures the HTTP adapter for multitenancy.

## Options

| Option | Type | Default | Description |
| --- | --- | --- | --- |
| `StoreTenantInHttpContextItems` | `bool` | `true` | Stores the resolved tenant in `HttpContext.Items`. |
| `HttpContextItemKey` | `string` | `CleanArchitecture.Extensions.Multitenancy.TenantContext` | Key used to store tenant context in `HttpContext.Items`. |
| `CorrelationIdHeaderName` | `string?` | `null` | Header name used to override the correlation ID. |
| `UseTraceIdentifierAsCorrelationId` | `bool` | `true` | Uses `HttpContext.TraceIdentifier` when no header is supplied. |

## Notes

- Correlation ID is resolved from `CorrelationIdHeaderName` when present; otherwise `TraceIdentifier` is used (if enabled).
- The resolved tenant context can be accessed via `HttpContextTenantExtensions.GetTenantContext()` when stored in items.
