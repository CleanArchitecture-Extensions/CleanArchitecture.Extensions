# Extension: Multitenancy Core

## Overview
Tenant context and resolution primitives for Clean Architecture solutions.

## When to use
- You need per-tenant isolation (data access, caching, authorization).
- You want pluggable tenant resolution strategies (host/header/route/claims).

## Prereqs & Compatibility
- Target .NET: TBD
- CleanArchitecture template: TBD
- Dependencies: TBD

## Install
`ash
# replace with actual package name when published
dotnet add src/YourProject/YourProject.csproj package CleanArchitecture.Extensions.Multitenancy
`

## Usage
- Register tenant resolution middleware/providers.
- Flow TenantId through application services and persistence.
- Add behaviors/filters to enforce tenant scope.

## Troubleshooting
- Ensure a tenant resolution strategy is configured; fallbacks should be explicit.
- Log resolved tenant identifiers and resolution source for diagnostics.

## Samples & Tests
- Link to runnable sample (add when available).
- Link to related tests once published.
