# Extension: Validation

## Overview
Validation pipeline and helpers built on FluentValidation for Clean Architecture solutions. Ships a configurable MediatR behavior, a template-shaped `ValidationException`, rule helpers, and a base validator that applies common conventions. Designed to be drop-in compatible with Jason Taylor’s template while enabling Result-based short-circuiting when desired.

## When to use
- You follow the template’s MediatR pipeline and want richer control over how validation failures surface (throw vs Result vs notify).
- You need shared validator conventions and basic rule helpers for identifiers, paging, and email without re-writing boilerplate.
- You want compatibility with existing `ValidationException` handling (dictionary of property -> messages) but also want to map failures to `Result`.

## Prereqs & Compatibility
- Target frameworks: `net10.0` (current).
- Dependencies: FluentValidation `11.9.2`, MediatR `12.2.0`.
- Template fit: register the behavior in Application after Authorization and before Performance, same signature as the template’s `ValidationBehaviour<,>`.

## Install
```bash
dotnet add src/YourProject/YourProject.csproj package CleanArchitecture.Extensions.Validation
```

## Usage

### Wire up validators and behavior (DI)
```csharp
services.AddValidatorsFromAssemblyContaining<Startup>(); // or your Application assembly marker
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Optional: configure strategy/options
services.Configure<ValidationOptions>(options =>
{
    options.Strategy = ValidationStrategy.Throw; // default to template behavior
    options.MaxFailures = 50;
    options.IncludeAttemptedValue = false;
});
```

### Strategies
- `Throw` (default): matches template; throws `ValidationException` with `Dictionary<string,string[]>`.
- `ReturnResult`: short-circuits with `Result.Failure`/`Result<T>.Failure` when handlers return `Result`.
- `Notify`: publishes via `IValidationNotificationPublisher` then either throws or returns `Result` based on `NotifyBehavior`.

Key options: `MaxFailures`, `IncludePropertyName`, `IncludeAttemptedValue`, `IncludePlaceholderValues`, `DefaultErrorCode`, `TraceId`, `ErrorCodeSelector`, `MessageFormatter`.

### Error model
- `ValidationError` maps FluentValidation failures to Core `Error`, carrying code, message, property, attempted value (opt-in), severity, and metadata.
- `ValidationException` is provided for compatibility with the template’s shape and can be thrown by the behavior.

### Base validator + rules
- `AbstractValidatorBase<T>` sets rule-level cascade to fail fast per chain.
- `Rules/CommonRules` helpers:
  - `NotEmptyTrimmed`
  - `EmailAddressBasic`
  - `OptionalEmailAddress`
  - `PositiveId`
  - `PageNumber`
  - `PageSize`

### Result short-circuit example
```csharp
var options = new ValidationOptions { Strategy = ValidationStrategy.ReturnResult };
var behavior = new ValidationBehavior<CreateTodo, Result<TodoVm>>(validators, options);
// When validation fails, the behavior returns Result<T>.Failure(errors) instead of throwing.
```

## Troubleshooting
- Behavior throws when handler return type is not `Result`/`Result<T>` and strategy is `ReturnResult`/`Notify` with `ReturnResult`; switch to `Throw` or update handler return types.
- Missing correlation/trace IDs: set `ValidationOptions.TraceId` from your pipeline or leave null to let downstream behaviors apply correlation.

## Samples & Tests
- See `tests/CleanArchitecture.Extensions.Validation.Tests` for strategy coverage and rule helper usage.
