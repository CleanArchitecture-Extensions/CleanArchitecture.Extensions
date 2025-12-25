# Core Guard Clauses

> Deprecated. This content is retained for reference only.

Guard clauses in the Core extension give you a consistent, testable way to enforce invariants without scattering `if/throw` statements across handlers and entities. They pair with the extension’s `Result` primitives so you can choose return-based validation (preferred for application flows) or exception-based validation (when you rely on middleware). This page explains how the Clean Architecture template handles validation today, why the extension adds more flexible guards, and how to integrate them with pipelines, Results, and domain logic.

## What the template already covers

Jason Taylor’s template leans on two mechanisms:

- **FluentValidation + MediatR ValidationBehaviour:** Validators throw `ValidationException` when rules fail. Controllers/middleware translate those exceptions into ProblemDetails responses.
- **Ad hoc checks:** Handlers and services sometimes do simple `if (string.IsNullOrWhiteSpace(...)) throw ...;` for quick guards. There is no common error shape or trace propagation.
- **Result type is minimal:** The template’s `Result` is `Succeeded + string[] Errors` and is mainly used in Identity flows; it does not pair with guard outcomes elsewhere.

This works for demos but makes it harder to:

- Return structured errors without throwing.
- Accumulate multiple guard failures for richer UX.
- Keep correlation/trace IDs attached to validation failures.
- Reuse guard logic in domain entities without bringing FluentValidation into the domain layer.

## Why use Core guard clauses

The Core package introduces a focused guard library that aligns with the template’s MediatR wiring while keeping your domain/application code technology-agnostic:

- **Strategy-driven:** Choose per-call whether to return `Result` failures (`ReturnFailure`), throw (`Throw`), or accumulate errors (`Accumulate`) into a shared sink.
- **Trace-friendly:** Guards accept `GuardOptions.TraceId`; failures propagate trace IDs into `Error` instances, staying aligned with correlation behaviors.
- **Result-native:** Guards produce `Result<T>` (or `Result`) so you can compose with `Map/Bind/Combine` instead of relying on exceptions.
- **Domain-ready:** No FluentValidation dependency; the API is simple enough to use inside aggregates while keeping error metadata consistent with the application layer.
- **Batch-aware:** `Accumulate` lets you gather multiple failures (e.g., CSV import) and report them together.

## API surface

Namespace: `CleanArchitecture.Extensions.Core.Guards`

Guards:

- `AgainstNull<T>(value, parameterName, options?)`
- `AgainstNullOrWhiteSpace(string?, parameterName, options?)`
- `AgainstOutOfRange<T>(value, min, max, parameterName, options?) where T : IComparable<T>`
- `AgainstUndefinedEnum<TEnum>(value, parameterName, options?) where TEnum : struct, Enum`
- `AgainstTooShort(string value, minLength, parameterName, options?)`
- `AgainstTooLong(string value, maxLength, parameterName, options?)`
- `Ensure(bool condition, code, message, options?)`

Support types:

- `GuardOptions` (strategy, error sink, exception factory, trace ID; plus `FromOptions(CoreExtensionsOptions, errorSink?)`).
- `GuardStrategy` (`ReturnFailure`, `Throw`, `Accumulate`).

Outputs:

- `Result<T>` or `Result` from the Core Result primitives, carrying `Error` objects with `Code`, `Message`, `TraceId`.

## Strategy modes explained

- **ReturnFailure (default):** Guards return `Result`/`Result<T>` failures. Handlers can short-circuit or combine errors. No exceptions are thrown.
- **Throw:** Guards throw immediately. If you prefer exception-driven flows (e.g., existing middleware for 400s), set `GuardOptions.Strategy = GuardStrategy.Throw` or configure `CoreExtensionsOptions.GuardStrategy`.
- **Accumulate:** Guard errors are added to an `ICollection<Error>` sink (provided via `GuardOptions.ErrorSink`), and a failure `Result` is returned. Use this for batch operations where you want to report all issues at once.

`ExceptionFactory` lets you shape exceptions when `Throw` is selected (e.g., custom domain exceptions).

## Comparing to template validation

- **FluentValidation Behaviour:** Runs after `AuthorizationBehaviour` and before `PerformanceBehaviour`. Throws `ValidationException` on first validation failure batch.
- **Core Guards:** Can run inside the handler or domain entity. When configured to return failures, they avoid exceptions and keep control in the handler. You can still use FluentValidation for complex cross-field rules while using guards for fast, local checks.
- **Blended approach:** Use guards for early, cheap validations (null/length/range) and FluentValidation for business rules that benefit from rich tooling and localization. Both can coexist because guards do not change pipeline signatures.

## Installation refresher

If you have not yet installed the Core extension:

```bash
dotnet add src/YourProject/YourProject.csproj package CleanArchitecture.Extensions.Core
```

Register `CoreExtensionsOptions` in DI so guards can inherit defaults:

```csharp
services.AddCleanArchitectureCore(options =>
{
    options.GuardStrategy = GuardStrategy.ReturnFailure;
});
```

## Quick-start examples (backed by the guard sample)

A runnable solution that demonstrates each strategy lives at `samples/CleanArchitecture.Extensions.Core.Guards.Sample`.

### Short-circuit on first failure (ReturnFailure)

`samples/CleanArchitecture.Extensions.Core.Guards.Sample/src/Application/Projects/Commands/CreateProject/CreateProject.cs`:

```csharp
var traceId = Guid.NewGuid().ToString("N");
var guardOptions = new CoreGuardOptions { TraceId = traceId };

var name = CoreGuard.AgainstNullOrWhiteSpace(request.Name, nameof(request.Name), guardOptions)
    .Ensure(n => n.Length <= MaxNameLength, new CoreResults.Error("projects.name.length", $"Name must be {MaxNameLength} characters or fewer.", traceId));

var description = string.IsNullOrWhiteSpace(request.Description)
    ? CoreResults.Result.Success<string?>(null, traceId)
    : CoreGuard.AgainstTooLong(request.Description, MaxDescriptionLength, nameof(request.Description), guardOptions)
        .Map(value => (string?)value.Trim(), traceId);

var budget = CoreGuard.Ensure(request.Budget >= 0,
    "projects.budget.range",
    "Budget cannot be negative.",
    guardOptions);

var validation = CoreResults.Result.Combine(name, description, budget);
if (validation.IsFailure)
{
    return CoreResults.Result.Failure<int>(validation.Errors, traceId);
}
```

- Guards stay branch-light until all cheap checks run, then short-circuit with a single `Result.Combine` failure.
- The handler continues on success to check duplicates and persist, preserving the same `traceId`.

### Accumulate multiple failures (batch import)

`samples/CleanArchitecture.Extensions.Core.Guards.Sample/src/Application/Projects/Commands/ImportProjects/ImportProjects.cs`:

```csharp
var sink = new List<CoreResults.Error>();
var guardOptions = new CoreGuardOptions
{
    Strategy = GuardStrategy.Accumulate,
    ErrorSink = sink,
    TraceId = traceId
};

for (var index = 0; index < incoming.Count; index++)
{
    var project = incoming[index];
    var beforeCount = sink.Count;

    var nameResult = CoreGuard.AgainstNullOrWhiteSpace(project.Name, nameof(project.Name), guardOptions)
        .Ensure(n => n.Length <= MaxNameLength, new CoreResults.Error("projects.name.length", $"Name must be {MaxNameLength} characters or fewer.", traceId));

    var budgetResult = CoreGuard.Ensure(project.Budget >= 0, "projects.budget.range", "Budget cannot be negative.", guardOptions);

    if (nameResult.IsSuccess && budgetResult.IsSuccess)
    {
        projects.Add(new Project(project.Name.Trim(), project.Description, project.Budget));
    }
    else
    {
        AddRowMetadata(sink, beforeCount, index + 1);
    }
}

if (sink.Count > 0)
{
    return CoreResults.Result.Failure<IReadOnlyList<int>>(sink, traceId);
}
```

- `GuardStrategy.Accumulate` keeps looping and decorates new errors with a `row` metadata entry so the client can show all issues at once.
- Only when the sink is empty do we add the batch to the DbContext and save.

### Throwing for exception-first pipelines

`samples/CleanArchitecture.Extensions.Core.Guards.Sample/src/Application/Projects/Commands/ArchiveProjectWithThrow/ArchiveProjectWithThrow.cs`:

```csharp
var guardOptions = new CoreGuardOptions
{
    Strategy = GuardStrategy.Throw,
    ExceptionFactory = error => new InvalidOperationException($"{error.Code}: {error.Message}")
};

var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

CoreGuard.AgainstNull(project, nameof(project), guardOptions);
CoreGuard.Ensure(!project!.IsArchived, "projects.archived", "Project is already archived.", guardOptions);

project.Archive(DateTimeOffset.UtcNow);
await _context.SaveChangesAsync(cancellationToken);
```

- When `Throw` is chosen, guards raise exceptions immediately. In the sample API, the endpoint catches and translates them to a 400 response to demonstrate exception-first wiring.

## Detailed guard behaviors

- **Trace propagation:** When `TraceId` is set in options, every guard-generated error carries it. If omitted, the guard produces errors without a trace; you can still attach a trace later when turning guard results into handler results.
- **Error codes:** Each guard uses a stable code:
  - `guard.null`
  - `guard.empty`
  - `guard.range`
  - `guard.enum`
  - `guard.length`
  - custom code for `Ensure` (you pass it).
    Use these codes for client-side localization or analytics.
- **Messages:** Default English messages are provided. If you need localization, wrap guards in a helper that maps codes to localized strings, or set `ExceptionFactory` to craft localized exceptions when throwing.
- **Range check semantics:** `AgainstOutOfRange` is inclusive (`minimum <= value <= maximum`). For exclusive checks, call it with adjusted bounds or add an `Ensure` with your own predicate.
- **Enum checks:** `AgainstUndefinedEnum` defends against undefined numeric casts or new values introduced without handler changes.
- **Length checks:** `AgainstTooShort` and `AgainstTooLong` operate on `string.Length`; for grapheme-aware length, add a custom guard using `Ensure`.

## Composition with Result helpers

Because guards return Results, you can compose them without `if` ladders:

```csharp
public Result<CheckoutRequest> Validate(CheckoutRequest input, GuardOptions options)
{
    return Guard.AgainstNullOrWhiteSpace(input.CustomerId, "CustomerId", options)
        .Bind(_ => Guard.AgainstOutOfRange(input.Items.Count, 1, 100, "Items.Count", options))
        .Bind(_ => Guard.Ensure(input.Currency == "USD", "checkout.currency", "Only USD supported", options))
        .Map(_ => input);
}
```

- `Bind` short-circuits on first failure.
- `Ensure` inserts custom predicate-based checks without new guard methods.
- `Map` wraps the validated input back into the result, preserving the trace ID.

## Using guards in domain entities

Domain entities should not depend on FluentValidation. Guards give you lightweight checks while keeping errors structured:

```csharp
public sealed class Money
{
    public string Currency { get; }
    public decimal Amount { get; }

    private Money(string currency, decimal amount)
    {
        Currency = currency;
        Amount = amount;
    }

    public static Result<Money> Create(string currency, decimal amount, GuardOptions options)
    {
        return Guard.AgainstNullOrWhiteSpace(currency, nameof(currency), options)
            .Bind(_ => Guard.AgainstOutOfRange(amount, 0m, 1_000_000m, nameof(amount), options))
            .Map(_ => new Money(currency.Trim().ToUpperInvariant(), amount));
    }
}
```

- The factory returns `Result<Money>`; handlers can propagate failures, aggregate them, or translate them into API responses without throwing.

## Integrating with MediatR pipelines

- **Ordering:** Use guards inside handlers before hitting infrastructure. If you prefer middleware-level handling, set strategy to `Throw` and let `UnhandledExceptionBehaviour` or your API middleware translate exceptions.
- **Correlation:** Run `CorrelationBehavior` early so `ILogContext.CorrelationId` is set. Pass that ID into `GuardOptions.TraceId` to keep guard errors aligned with logs.
- **PerformanceBehavior:** Guards are synchronous and cheap; they do not materially affect elapsed timing unless used in tight loops on large collections (batch validation). In those cases, consider aggregating and emitting a single performance log.

## Working with FluentValidation

- **Complementary roles:** Use guards for fast, mechanical checks (null/length/range) and FluentValidation for richer, cross-field rules or localized error messages.
- **Avoid double work:** If FluentValidation already covers a rule, do not duplicate it with guards unless you want early short-circuiting before expensive operations (e.g., DB calls).
- **Shared error shape:** You can map `ValidationFailure` to `Error` to stay consistent with guard outputs:

```csharp
var errors = failures.Select(f =>
    new Error($"validation.{f.PropertyName.ToLowerInvariant()}", f.ErrorMessage, traceId));
return Result.Failure<TResponse>(errors, traceId);
```

## Configuration via CoreExtensionsOptions

`GuardOptions.FromOptions(CoreExtensionsOptions options, ICollection<Error>? errorSink = null)` lets you centralize defaults:

- `GuardStrategy` (default `ReturnFailure`).
- `TraceId` default.
- Injected `ErrorSink` when you want accumulation.
  Set `Extensions:Core:GuardStrategy` in configuration to change the default behavior across the app, then override per call as needed.

## Error accumulation patterns

For scenarios where you must present all issues (e.g., admin bulk imports), combine `Accumulate` with `Result.Combine`:

```csharp
var sink = new List<Error>();
var options = new GuardOptions { Strategy = GuardStrategy.Accumulate, ErrorSink = sink, TraceId = traceId };

Guard.AgainstTooShort(user.FirstName, 2, "FirstName", options);
Guard.AgainstTooShort(user.LastName, 2, "LastName", options);
Guard.Ensure(user.Email.Contains('@'), "user.email", "Email is invalid", options);

return sink.Count == 0
    ? Result.Success(options.TraceId)
    : Result.Failure(sink, options.TraceId);
```

- You can still return a single `Result` to the caller while surfacing all errors.

## HTTP/API translation

Pair guard-produced errors with ProblemDetails:

```csharp
var result = await mediator.Send(command, ct);
if (result.IsFailure)
{
    var problems = result.Errors.Select(e => new { e.Code, e.Message, e.TraceId, e.Metadata });
    return Results.Problem(
        title: "Validation failed",
        statusCode: StatusCodes.Status400BadRequest,
        detail: "One or more validation errors occurred.",
        extensions: new Dictionary<string, object?>
        {
            ["errors"] = problems,
            ["traceId"] = result.TraceId
        });
}
return Results.Ok(new { result.Value, traceId = result.TraceId });
```

- Clients get consistent codes/messages and a trace ID for troubleshooting.

## Testing guards

- **Deterministic:** Guards have no time dependency. You can assert on `IsSuccess`, `Errors.Count`, `Error.Code`, and `TraceId`.
- **Example:**

```csharp
[Fact]
public void AgainstTooShort_ReturnsError_WhenBelowThreshold()
{
    var options = new GuardOptions { TraceId = "unit-1" };
    var result = Guard.AgainstTooShort("ab", 3, "Name", options);

    result.IsFailure.Should().BeTrue();
    result.Errors.Single().Code.Should().Be("guard.length");
    result.TraceId.Should().Be("unit-1");
}
```

- **Accumulate mode:** Assert that the error sink contains all errors and that the returned `Result` is failure.

## Performance considerations

- Guards are lightweight and allocation-friendly (simple Error structs, minimal collections). The main cost in accumulation mode is list growth; reuse sinks where appropriate for large batches.
- For high-frequency hot paths, prefer `ReturnFailure` to avoid exception costs. Reserve `Throw` for places where exceptions are part of your existing contract.

## Extending guards

You can add custom guards alongside the built-ins by wrapping `Ensure`:

```csharp
public static class GuardExtras
{
    public static Result<T> AgainstWeekend<T>(DateOnly date, string parameterName, GuardOptions? options = null)
    {
        var isWeekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        return Guard.Ensure(!isWeekend, $"date.{parameterName}.weekend", $"{parameterName} cannot fall on a weekend.", options)
            .Map(_ => (T)(object)date); // or return Result.Success(date, options?.TraceId)
    }
}
```

Keep codes prefixed by the domain area to avoid collisions (`inventory.*`, `billing.*`, etc.).

## Adoption playbook

1. **Configure defaults:** Set `Extensions:Core:GuardStrategy` to `ReturnFailure` (recommended). Enable a correlation-aware `TraceId` if you have one.
2. **Update new handlers:** Use guards at the top of handlers to validate inputs, returning `Result<T>` on failure.
3. **Add accumulation for batch workflows:** Introduce `Accumulate` + sinks where you previously looped over items with ad hoc validation.
4. **Bridge to existing exception flows:** Where middleware expects exceptions, set strategy to `Throw` or wrap guard results in exceptions until you migrate clients.
5. **Refine codes/messages:** Standardize error codes across teams. Document them near your domain services or in a shared constants file.

## Reference: Methods and codes

- `AgainstNull` → `guard.null`
- `AgainstNullOrWhiteSpace` → `guard.empty`
- `AgainstOutOfRange` → `guard.range`
- `AgainstUndefinedEnum` → `guard.enum`
- `AgainstTooShort` → `guard.length`
- `AgainstTooLong` → `guard.length`
- `Ensure` → user-supplied code

## Related docs

- [Core extension overview](./core.md) for pipeline behaviors, logging, time, domain events, and options.
- [Core Result Primitives](./core-result-primitives.md) to see how guard outputs compose with results and downstream handlers.
- [Validation extension](./validation.md) when you need FluentValidation-driven behaviors in addition to guards.
