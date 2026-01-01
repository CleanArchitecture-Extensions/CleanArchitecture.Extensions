# Contributing

We keep the upstream template pristine and ship everything as opt-in packages. Contributions must follow that contract and stay in sync with docs and tests.

## Principles

- **Template-first**: mirror Jason Taylor's conventions. Do not modify the upstream template.
- **Opt-in by default**: minimal required configuration, clear defaults, easy removal.
- **Docs and tests**: every behavior change updates docs under `docs/` and tests under `tests/`.
- **Separation of concerns**: Application stays clean; Infrastructure and host adapt via composition.

## Workflow

1) Read the relevant design doc in `HighLevelDocs/Domain*/CleanArchitecture.Extensions.*.md`.
2) Make changes inside `CleanArchitecture.Extensions/` (src/tests/samples/docs/build).
3) Update docs and ensure navigation links remain valid.
4) Add or update tests for the package you touched.

## Documentation style

- Follow the extension template: overview, when to use, compat, install, usage, troubleshooting.
- Use fenced code blocks with language tags (`csharp`, `powershell`, `json`).
- Prefer short, runnable snippets and keep them under ~30 lines.
- Use Markdown links for external URLs (e.g., `[Quickstart](https://...)`).

## Testing expectations

- Add unit tests for behaviors and configuration logic.
- Add integration tests for EF Core and ASP.NET Core adapters when behavior changes.
- Keep test names aligned with existing naming conventions.

## Local docs preview

```powershell
python -m venv .venv
. .venv/Scripts/Activate.ps1
pip install -r docs/requirements.txt
mkdocs serve
```
