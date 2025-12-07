# CleanArchitecture.Extensions.Validation

FluentValidation-powered MediatR pipeline aligned with CleanArchitecture.Extensions.Core results.

- Executes validators in the MediatR pipeline and surfaces failures as exceptions or `Result` responses.
- Optional notification publishing through `IValidationNotificationPublisher` for UI or logging hooks.
- Correlation-aware summaries: plugs into `ILogContext`/`IAppLogger` and uses `SeverityLogLevels` to choose log levels per failure.
- Tunable options for fail-fast limits, message formatting, notify/return/throw strategies, and trace ID propagation from Core.
- Ships with SourceLink, XML docs, and snupkg symbols for debugger-friendly packages.

## Install

```powershell
dotnet add package CleanArchitecture.Extensions.Validation --version 0.1.1-preview.1
```

## Usage

```csharp
using CleanArchitecture.Extensions.Validation.Behaviors;
using CleanArchitecture.Extensions.Validation.Options;
using CleanArchitecture.Extensions.Core.Logging;
using FluentValidation;
using MediatR;

services.AddValidatorsFromAssemblyContaining<CreateOrderValidator>();
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

services.Configure<ValidationOptions>(options =>
{
    options.Strategy = ValidationStrategy.ReturnResult;
    options.IncludePropertyName = true;
    options.SeverityLogLevels[Severity.Error] = LogLevel.Warning;
    options.LogValidationFailures = true;
});
```

Implement `IValidationNotificationPublisher` if you want to capture validation failures without throwing.

## Rule helpers

- `NotEmptyTrimmed`
- `EmailAddressBasic`
- `OptionalEmailAddress`
- `PositiveId`
- `PageNumber`
- `PageSize`
- `PhoneE164`
- `UrlAbsoluteHttpHttps`
- `CultureCode`
- `SortExpression` (allowed field whitelist)
- Tenant-aware rules (planned alongside the Multitenancy module)

## Target frameworks

- net8.0
- net10.0
