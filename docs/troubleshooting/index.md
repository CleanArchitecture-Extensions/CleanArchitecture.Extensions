# Troubleshooting

Common issues and fixes for the current extensions.

- Caching not working: verify the behavior is registered and the request matches the cache predicate.
- Tenant not resolved: confirm the resolution context is populated and provider order is correct.
- Tenant validation failures: ensure `ITenantInfoStore`/`ITenantInfoCache` is registered when validation is enabled.
- EF Core enforcement errors: confirm tenant context is available for writes.

See also:

- [Multitenancy troubleshooting](multitenancy.md)
- [Caching extension](../extensions/caching.md)
- [Multitenancy core](../extensions/multitenancy-core.md)
