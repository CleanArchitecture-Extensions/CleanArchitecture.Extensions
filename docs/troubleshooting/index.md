# Troubleshooting

Common issues and fixes (expand as features land).

- Extension fails to start: enable debug logs and check config keys.
- Tenant not resolved: verify provider order (host/header/route/claims) and add logging for resolution steps.
- Caching mismatches: confirm cache keys include tenant/user when required.
