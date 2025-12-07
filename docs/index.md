# CleanArchitecture.Extensions

Extensions for Jason Taylor's Clean Architecture template, delivered as opt-in NuGet packages so the upstream repo stays pristine. Shipped today: Core + Validation (preview). Everything else is documented as work-in-progress with designs in `HighLevelDocs/*`.

What to expect:
- Quickstarts and install guides to wire extensions without forking the template.
- Deep-dive pages per extension with compat, install, usage, troubleshooting, samples, and tests.
- Recipes and samples that map directly to Clean Architecture projects.
- Roadmap and release notes to track what is shipping next.

Working style:
- Template-first: keep the original template untouched; plug in via packages, behaviors, middleware, and adapters.
- Docs-first: every shipped capability gets a page, sample, and recipe.
- Sample-first: runnable projects in `samples/` mirror the docs and README guidance.

## Quick links
- Getting started: [Quickstart](getting-started/quickstart.md) · [Installation](getting-started/installation.md)
- Concepts: [Architecture fit](concepts/architecture-fit.md) · [Composition & invariants](concepts/composition.md)
- Catalog: [Extensions index](extensions/index.md) (Core, Validation, Multitenancy placeholder, more to come)
- Recipes: [Authentication](recipes/authentication.md) · [Caching](recipes/caching.md)
- Samples: [Samples index](samples/index.md)
- Reference & Ops: [Configuration](reference/configuration.md) · [Troubleshooting](troubleshooting/index.md) · [Release notes](release-notes/index.md) · [Roadmap](roadmap.md)
- Contributing: [How to contribute](contributing/index.md)
