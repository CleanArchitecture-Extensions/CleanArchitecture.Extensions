# Installation

## Prerequisites
- .NET 8 SDK (for consuming extensions and running samples/tests)
- git (to pull the repo)
- Python 3.10+ (only if you want to build/serve docs locally)

## Install an extension (placeholder)
Replace names with real packages when published.
`ash
 dotnet add src/YourProject/YourProject.csproj package CleanArchitecture.Extensions.Sample
`

## Local docs tooling (optional)
`ash
python -m venv .venv
. .venv/Scripts/Activate.ps1
pip install -r docs/requirements.txt
mkdocs serve
`
"@;

  'docs/concepts/architecture-fit.md' = @"
# Architecture Fit

How extensions align with Jason Taylor's Clean Architecture template.

- Extensions stay out of the template repo; they plug in via packages, configuration, and middleware/behaviors.
- Favor composition over modification: add pipeline behaviors, decorators, and adapters rather than changing core layers.
- Preserve boundaries: respect domain/application/infrastructure/UI separation and dependency direction.
- Match conventions: naming, folder structure, and style should mirror Jason's reference repo (see ../JasonTaylorCleanArchitecture).
- Keep optionality: each extension should be opt-in, with clear defaults and minimal required configuration.
