# CleanArchitecture.Extensions.Validation

FluentValidation-powered MediatR pipeline aligned with CleanArchitecture.Extensions.Core results.

- Executes validators in the MediatR pipeline and surfaces failures as exceptions or `Result` responses.
- Optional notification publishing through `IValidationNotificationPublisher` for UI or logging hooks.
- Tunable options for fail-fast limits, message formatting, and notify/return/throw strategies.
- Ships with SourceLink, XML docs, and snupkg symbols for debugger-friendly packages.

## Install

```powershell
dotnet add package CleanArchitecture.Extensions.Validation --version 0.1.0-preview.1
```

## Usage

```csharp
using CleanArchitecture.Extensions.Validation.Behaviors;
using CleanArchitecture.Extensions.Validation.Options;
using FluentValidation;
using MediatR;

services.AddValidatorsFromAssemblyContaining<CreateOrderValidator>();
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

services.Configure<ValidationOptions>(options =>
{
    options.Strategy = ValidationStrategy.ReturnResult;
    options.IncludePropertyName = true;
});
```

Implement `IValidationNotificationPublisher` if you want to capture validation failures without throwing.

## Target frameworks

- net8.0
- net10.0
