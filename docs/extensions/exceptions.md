# Extension: Exceptions

## Overview

Exception catalog, base exception types, redaction helpers, and a MediatR pipeline behavior that standardize how Clean Architecture solutions translate and log failures. Exceptions resolve to stable codes and severities, can be converted into `Result`/`Result<T>` (and the template’s static `Result.Failure(IEnumerable<string>)`), and flow correlation/trace IDs into logs and error metadata.

## When to use

- You want predictable error codes/messages for handlers, APIs, background jobs, and tests instead of ad-hoc exceptions.
- You need to convert exceptions into `Result`/`Result<T>` without changing handler signatures.
- You rely on Jason Taylor’s template `Result` shape and want compatibility without forking.
- You want centralized redaction and retry/transient classification guidance.

## Prereqs & Compatibility

- Target frameworks: `net10.0`.
- Dependencies: MediatR `14.0.0`, Microsoft.Extensions.Options `10.0.0`, CleanArchitecture.Extensions.Core.
- Template fit: register `ExceptionWrappingBehavior<,>` near the top of the MediatR pipeline (after correlation/logging, before authorization/validation/performance). It replaces the template’s `UnhandledExceptionBehaviour<,>` while keeping handler signatures intact.

## Install

```bash
dotnet add src/YourProject/YourProject.csproj package CleanArchitecture.Extensions.Exceptions
```

## Usage

### Register catalog and behavior

```csharp
using CleanArchitecture.Extensions.Exceptions.Behaviors;
using CleanArchitecture.Extensions.Exceptions.Options;
using CleanArchitecture.Extensions.Exceptions.Catalog;
using MediatR;

services.AddTransient<IExceptionCatalog, ExceptionCatalog>(); // optional: use defaults or configure below
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionWrappingBehavior<,>));

services.Configure<ExceptionHandlingOptions>(options =>
{
    options.ConvertToResult = true; // default: maps to Result/Result<T>/template Result
    options.IncludeExceptionDetails = false; // prefer catalog messages in prod
    options.RedactSensitiveData = true;
    options.RethrowCancellationExceptions = true; // OperationCanceledException is bypassed by default
});
```

### Base exceptions and codes

- `NotFoundException` (`ERR.NOT_FOUND`, 404)
- `ConflictException` (`ERR.CONFLICT`, 409)
- `ForbiddenException`/`ForbiddenAccessException` (`ERR.SECURITY.FORBIDDEN`, 403)
- `UnauthorizedException` (`ERR.SECURITY.UNAUTHORIZED`, 401)
- `DomainException` (`ERR.DOMAIN.GENERIC`)
- `ConcurrencyException` (`ERR.CONCURRENCY`, transient, 409)
- `TransientException` (`ERR.TRANSIENT`, transient, 503)
- Validation + cancellation fallbacks (`ERR.VALIDATION`, `ERR.CANCELLED`) plus a generic `ERR.UNKNOWN`

### Result conversion and logging

- When `ConvertToResult` is `true`, exceptions map to `Result`/`Result<T>` errors (or the template `Result.Failure(IEnumerable<string>)` when the response type matches the template shape).
- Trace/correlation IDs flow into `Error.TraceId` using `ExceptionHandlingOptions.TraceId`, `ILogContext`, or `CoreExtensionsOptions.TraceId`.
- `IncludeExceptionDetails` toggles whether raw exception messages flow to errors/logs; catalog messages remain stable, and redaction is applied when `RedactSensitiveData` is enabled.
- `RethrowExceptionTypes` defaults to cancellation + validation exceptions; add your own to bypass wrapping.

### Customize the catalog and transient classification

```csharp
using CleanArchitecture.Extensions.Exceptions.BaseTypes;
using CleanArchitecture.Extensions.Exceptions.Catalog;
using CleanArchitecture.Extensions.Exceptions.Options;
using System.Net;

services.Configure<ExceptionCatalogOptions>(options =>
{
    options.Descriptors.Add(new ExceptionDescriptor(
        typeof(HttpRequestException),
        ExceptionCodes.Transient,
        "Downstream HTTP request failed.",
        ExceptionSeverity.Warning,
        isTransient: true,
        statusCode: HttpStatusCode.BadGateway));
});

var isTransient = ExceptionClassifier.IsTransient(exception, catalog);
```

### Redaction defaults

`ExceptionRedactor` scrubs bearer tokens, passwords, secrets, cookies, and basic email/token patterns from messages and metadata when `RedactSensitiveData` is enabled. Override by supplying your own redactor instance to the behavior if you need stricter rules.

## Troubleshooting

- **Exceptions still rethrow**: ensure the type isn’t listed in `ExceptionHandlingOptions.RethrowExceptionTypes` and that `RethrowCancellationExceptions` isn’t forcing a bypass.
- **Generic messages**: set the exception message on your application/domain exceptions; catalog defaults are used when messages are empty. Enable `IncludeExceptionDetails` for debugging.
- **No Result conversion**: return types must be `Result`, `Result<T>`, or the template `Result` with a static `Failure(IEnumerable<string>)` method; otherwise the behavior will rethrow after logging.

## Samples & Tests

- Tests: `tests/CleanArchitecture.Extensions.Exceptions.Tests`.
- Samples: follow the pipeline samples under `samples/CleanArchitecture.Extensions.Core.*` until a dedicated Exceptions sample lands.
