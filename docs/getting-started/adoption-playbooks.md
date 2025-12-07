# Adoption playbooks (from README Appendix B)

## Appendix B — scenario matrices and play-patterns

## Telemetry-first adoption {#telemetry-first-adoption}

Start with Core correlation, logging, and performance behaviors to light up request tracing in development. Layer Observability (when available) to export OTEL traces without touching handlers. Pair with Validation to surface meaningful errors early. This pattern suits teams that need better diagnostics before expanding functionality.

How to get started:

- Identify which extensions are already available (Core and Validation today) and wire them following the quickstart.
- Read the matching HighLevelDocs entries for upcoming modules to understand required seams in your code.
- Sketch pipeline ordering and configuration toggles so adoption is a config change, not a refactor.
- Run the samples closest to your scenario and adapt them into your solution structure.
- Track the docs site and release notes for module-specific guidance as packages ship.

Success signals to look for:

- Logs and traces include correlation IDs and, when applicable, tenant identifiers.
- Validation errors are predictable and localized to the entry points where they belong.
- Pipeline ordering is explicit and documented in your solution.
- Teams can add or remove an extension via NuGet plus configuration without code churn.
- Samples and docs in this repo stay aligned with what you have in production.

## Migration from template Result {#migration-from-template-result}

Use Core's LegacyResult shims to bridge from the template's bool+errors shape to the richer Result model. Migrate handlers incrementally while keeping controllers untouched. Add Validation with ReturnResult strategy to avoid exceptions where you want explicit outcomes. This keeps production stable while you modernize internals.

How to get started:

- Identify which extensions are already available (Core and Validation today) and wire them following the quickstart.
- Read the matching HighLevelDocs entries for upcoming modules to understand required seams in your code.
- Sketch pipeline ordering and configuration toggles so adoption is a config change, not a refactor.
- Run the samples closest to your scenario and adapt them into your solution structure.
- Track the docs site and release notes for module-specific guidance as packages ship.

Success signals to look for:

- Logs and traces include correlation IDs and, when applicable, tenant identifiers.
- Validation errors are predictable and localized to the entry points where they belong.
- Pipeline ordering is explicit and documented in your solution.
- Teams can add or remove an extension via NuGet plus configuration without code churn.
- Samples and docs in this repo stay aligned with what you have in production.

## SaaS with tenant isolation {#saas-with-tenant-isolation}

Adopt multitenancy core for tenant resolution and enforcement once it lands. Pair it with EFCore and AspNetCore adapters to enforce tenant boundaries in data and HTTP layers. Add RateLimiting and Caching using tenant-aware strategies. When billing arrives, connect Payments and Provisioning to automate onboarding and subscription state.

How to get started:

- Identify which extensions are already available (Core and Validation today) and wire them following the quickstart.
- Read the matching HighLevelDocs entries for upcoming modules to understand required seams in your code.
- Sketch pipeline ordering and configuration toggles so adoption is a config change, not a refactor.
- Run the samples closest to your scenario and adapt them into your solution structure.
- Track the docs site and release notes for module-specific guidance as packages ship.

Success signals to look for:

- Logs and traces include correlation IDs and, when applicable, tenant identifiers.
- Validation errors are predictable and localized to the entry points where they belong.
- Pipeline ordering is explicit and documented in your solution.
- Teams can add or remove an extension via NuGet plus configuration without code churn.
- Samples and docs in this repo stay aligned with what you have in production.

## Event-driven integration {#event-driven-integration}

Lean on Core domain events today and plan for MessageBus and Outbox patterns to publish integration events with correlation IDs and tenant context. Observability will carry traces across process boundaries. Notifications can react to events for user-facing messages. This pattern is suited for teams integrating with other services via asynchronous pipelines.

How to get started:

- Identify which extensions are already available (Core and Validation today) and wire them following the quickstart.
- Read the matching HighLevelDocs entries for upcoming modules to understand required seams in your code.
- Sketch pipeline ordering and configuration toggles so adoption is a config change, not a refactor.
- Run the samples closest to your scenario and adapt them into your solution structure.
- Track the docs site and release notes for module-specific guidance as packages ship.

Success signals to look for:

- Logs and traces include correlation IDs and, when applicable, tenant identifiers.
- Validation errors are predictable and localized to the entry points where they belong.
- Pipeline ordering is explicit and documented in your solution.
- Teams can add or remove an extension via NuGet plus configuration without code churn.
- Samples and docs in this repo stay aligned with what you have in production.

## Compliance and audit readiness {#compliance-and-audit-readiness}

Plan for Audit to capture who did what and when, using Core's time and correlation primitives. Pair with Authorization and Settings to enforce policies and runtime overrides. Localization will help surface user-facing messages in the right language, and FeatureFlags can gate risky changes. Samples will include audit trails for commands and data changes.

How to get started:

- Identify which extensions are already available (Core and Validation today) and wire them following the quickstart.
- Read the matching HighLevelDocs entries for upcoming modules to understand required seams in your code.
- Sketch pipeline ordering and configuration toggles so adoption is a config change, not a refactor.
- Run the samples closest to your scenario and adapt them into your solution structure.
- Track the docs site and release notes for module-specific guidance as packages ship.

Success signals to look for:

- Logs and traces include correlation IDs and, when applicable, tenant identifiers.
- Validation errors are predictable and localized to the entry points where they belong.
- Pipeline ordering is explicit and documented in your solution.
- Teams can add or remove an extension via NuGet plus configuration without code churn.
- Samples and docs in this repo stay aligned with what you have in production.

## Developer experience at scale {#developer-experience-at-scale}

Templates and CLI will let teams scaffold new services with extensions pre-wired. Testing utilities will standardize handler tests with deterministic clocks and fake loggers. NuGetPackaging and SourceLinkAndSymbols keep packages consistent. This path is for organizations standardizing on Clean Architecture and needing reproducible setups across squads.

How to get started:

- Identify which extensions are already available (Core and Validation today) and wire them following the quickstart.
- Read the matching HighLevelDocs entries for upcoming modules to understand required seams in your code.
- Sketch pipeline ordering and configuration toggles so adoption is a config change, not a refactor.
- Run the samples closest to your scenario and adapt them into your solution structure.
- Track the docs site and release notes for module-specific guidance as packages ship.

Success signals to look for:

- Logs and traces include correlation IDs and, when applicable, tenant identifiers.
- Validation errors are predictable and localized to the entry points where they belong.
- Pipeline ordering is explicit and documented in your solution.
- Teams can add or remove an extension via NuGet plus configuration without code churn.
- Samples and docs in this repo stay aligned with what you have in production.

## Inspiration and gratitude

This project exists because of Jason Taylor's Clean Architecture template. The template gives teams a clear starting point; this repository gives them an ecosystem to grow without forking. If you are new here, start by reading Jason's repository: https://github.com/jasontaylordev/CleanArchitecture. Then come back and plug in only what you need.

## Next steps for readers

- If you want to build with what exists today: install Core and Validation, run the samples, read the Core and Validation docs, and wire the behaviors into your pipeline.
- If you want to contribute: pick a work-in-progress module, read its HighLevelDocs design, and start with docs and sample scaffolding before coding.
- If you want to keep tabs: watch the repo and the GitHub Pages site for release notes and roadmap updates.

---

This README is intentionally comprehensive and long-form to capture newcomers at first glance and give veterans the depth they need. The links above take you directly to the working docs, samples, and design plans. Use what you need today, and help shape what ships next.
