# CleanArchitecture.Extensions

[![Docs](https://github.com/CleanArchitecture-Extensions/CleanArchitecture.Extensions/actions/workflows/docs.yml/badge.svg?branch=main)](https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions)
[![CodeQL](https://github.com/CleanArchitecture-Extensions/CleanArchitecture.Extensions/actions/workflows/codeql.yml/badge.svg?branch=main)](https://github.com/CleanArchitecture-Extensions/CleanArchitecture.Extensions/actions/workflows/codeql.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Extensions ecosystem inspired by Jason Taylor's Clean Architecture template, shipped as opt-in NuGet packages so you can keep the upstream template pristine.

- Start fast with the [Quickstart](https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/getting-started/quickstart/).
- Browse the [Extensions Catalog](https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/extensions/) to opt into the pieces you need.
- Follow task-based [Recipes](https://cleanarchitecture-extensions.github.io/CleanArchitecture.Extensions/recipes/) and runnable samples.

## Repo layout

- `src/` extension packages targeting `CleanArchitecture.Extensions.sln`
- `tests/` coverage for each extension package
- `samples/` runnable scenarios
- `docs/` MkDocs source published to GitHub Pages

## CI and quality

- `docs.yml`: builds and publishes the docs site to `gh-pages`.
- `codeql.yml`: CodeQL static analysis on pushes, pull requests, and weekly schedule.
