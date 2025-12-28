# Contributing

We keep the upstream Jason Taylor template pristine and ship everything as opt-in packages. Contributions must follow that contract and stay in sync with the docs and samples.

## Principles
- Template-first: mirror Jason Taylor’s conventions (folder layout, naming, MediatR pipeline ordering). Do **not** modify `JasonTaylorCleanArchitecture/`.
- Extension = opt-in: minimal required config, no surprises; keep dependencies light.
- Docs/samples-first: every behavior change lands with updated docs under `docs/` and, where applicable, a sample under `samples/`.
- Tests-close-to-code: add/adjust tests in `tests/` for the package you touch.

## Workflow
1. Pick the design doc: read the matching `HighLevelDocs/Domain*/CleanArchitecture.Extensions.*.md` before coding.
2. Branch in this repo; keep changes inside `CleanArchitecture.Extensions/` solution (src/tests/samples/docs/build).
3. Implement + test: add or update unit/integration tests for your changes.
4. Update docs: extension page, recipes, reference, and roadmap if scope changes. Keep nav links valid.
5. Preview docs locally (optional):  
   ```powershell
   python -m venv .venv
   . .venv/Scripts/Activate.ps1
   pip install -r docs/requirements.txt
   mkdocs serve
   ```
6. Run relevant samples/tests where applicable and note any manual steps in your PR description.

## Style (docs)
- Follow the documentation strategy templates (overview → when to use → compat → install → usage → troubleshooting → samples/tests).
- Use fenced code blocks with language tags (`bash`, `powershell`, `csharp`, `json`).
- Prefer snippets from source to avoid drift; keep examples short and runnable.
- Keep compatibility info current (template version, target frameworks, dependencies).
- Format external links as Markdown links, for example `- [Roadmap](https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/roadmap/)` (avoid `Label: URL`).
