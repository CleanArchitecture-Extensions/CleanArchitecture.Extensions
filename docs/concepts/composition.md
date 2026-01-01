# Composition and invariants

Use the extensions together without losing clarity or breaking the template's ordering.

## Invariants

- **Explicit wiring**: every behavior or middleware is registered intentionally.
- **No hidden dependencies**: cross-extension integration uses explicit opt-in hooks.
- **Fail fast**: invalid configuration should be detected early (startup or first request).
- **Keep handlers clean**: cross-cutting concerns live in pipeline behaviors, filters, or adapters.

## Recommended composition order

### MediatR pipeline (Application)

A common order that aligns with the template is:

1. Logging pre-processors
2. Exception handling
3. Authorization
4. Validation
5. Multitenancy validation/enforcement
6. Caching behavior
7. Performance logging

Adjust for your template, but keep validation/enforcement before caching to avoid caching invalid requests.

### HTTP middleware (Web/API)

- Correlation and localization (if used)
- Tenant resolution
- Authentication/authorization
- MVC/minimal API endpoints

If you rely on claim-based tenant resolution, place authentication before the tenant resolution middleware.

## Cross-extension integration

- **Multitenancy + caching**: call `AddCleanArchitectureMultitenancyCaching` to include tenant IDs in cache keys.
- **Multitenancy + EF Core**: ensure `TenantSaveChangesInterceptor` is registered and your DbContext uses tenant-aware model customization.

## Compatibility notes

- Keep `MultitenancyOptions` and EF Core options aligned (tenant ID property name, global entities, isolation mode).
- Use consistent tenant identifiers across resolution sources (header, route, host) to avoid ambiguity.
