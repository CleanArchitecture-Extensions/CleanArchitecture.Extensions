# CleanArchitecture.Extensions.Exceptions

Exception catalog and MediatR pipeline behavior that standardize how Clean Architecture applications translate, log, and classify failures.

- Base exception types with stable codes (`NotFoundException`, `ConflictException`, `ForbiddenAccessException`, `UnauthorizedException`, `ConcurrencyException`, `TransientException`, `DomainException`).
- Catalog + options to map exceptions to codes, HTTP semantics, severity, and retryability while keeping handler code clean.
- `ExceptionWrappingBehavior<TRequest,TResponse>` to translate unhandled exceptions into `Result`/`Result<T>` and enrich logs with correlation IDs.
- Redaction helpers to scrub sensitive data before logging and a classifier to flag retryable/transient failures.

## Install

```powershell
dotnet add package CleanArchitecture.Extensions.Exceptions --version 0.1.1-preview.1
```

## Usage

Register the behavior in the MediatR pipeline (after validation, before performance logging) and tune the options if needed:

```csharp
using CleanArchitecture.Extensions.Exceptions.Behaviors;
using CleanArchitecture.Extensions.Exceptions.Options;
using MediatR;

services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionWrappingBehavior<,>));

services.Configure<ExceptionHandlingOptions>(options =>
{
    options.IncludeExceptionDetails = false;
    options.RethrowExceptionTypes.Add(typeof(OperationCanceledException));
});
```

When handlers return `Result`/`Result<T>`, the behavior converts catalogued exceptions into failure results so controllers can return `ProblemDetails` consistently. Configure redaction and retry classification via `ExceptionHandlingOptions` and the catalog if you need stricter policies.
