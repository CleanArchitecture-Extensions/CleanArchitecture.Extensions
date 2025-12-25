# CleanArchitecture.Extensions.Exceptions

> Deprecated. Use the template's built-in exception handling and Web problem details mapping.

Exception catalog and MediatR pipeline behavior that standardize how Clean Architecture applications translate, log, and classify failures.

- Base exception types with stable codes (`NotFoundException`, `ConflictException`, `ForbiddenAccessException`, `UnauthorizedException`, `ConcurrencyException`, `TransientException`, `DomainException`).
- Catalog + options to map exceptions to codes, HTTP semantics, severity, and retryability while keeping handler code clean.
- `ExceptionWrappingBehavior<TRequest,TResponse>` to translate unhandled exceptions into `Result`/`Result<T>` and enrich logs with correlation IDs.
- Redaction helpers to scrub sensitive data before logging and a classifier to flag retryable/transient failures.

## Install

```powershell
dotnet add package CleanArchitecture.Extensions.Exceptions --version <latest-version>
```

Check NuGet.org for the latest stable or preview version before installing.

## Usage

Register the behavior in the MediatR pipeline (after validation, before performance logging) and tune the options if needed:

```csharp
using System.Net;
using CleanArchitecture.Extensions.Exceptions;
using CleanArchitecture.Extensions.Exceptions.Options;
using CleanArchitecture.Extensions.Exceptions.BaseTypes;
using CleanArchitecture.Extensions.Exceptions.Catalog;
using MediatR;

services.AddCleanArchitectureExceptions(options =>
{
    options.IncludeExceptionDetails = false;
    options.RethrowExceptionTypes.Add(typeof(OperationCanceledException));
    options.EnvironmentName = env.EnvironmentName; // include details/stack in configured environments (e.g., Development)
    options.IncludeStackTrace = false;
    options.StatusCodeOverrides["ERR.DOMAIN.GENERIC"] = HttpStatusCode.UnprocessableEntity;
});

services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddCleanArchitectureExceptionsPipeline();
});

// Optional: configure catalog overrides
services.Configure<ExceptionCatalogOptions>(catalog =>
{
    catalog.Descriptors.Add(new ExceptionDescriptor(
        typeof(CustomDomainException),
        "ERR.DOMAIN.CUSTOM",
        "Custom domain failure",
        ExceptionSeverity.Error));
});
```

When handlers return `Result`/`Result<T>`, the behavior converts catalogued exceptions into failure results so controllers can return `ProblemDetails` consistently. Configure redaction and retry classification via `ExceptionHandlingOptions` and the catalog if you need stricter policies.
