# Core Result Primitives

The Core extension ships a richer `Result` model than the one inside Jason Taylor’s Clean Architecture template. This page explains what the template provides today, why the extension adds more, how to use the upgraded primitives, and how to adopt them incrementally without fighting the template’s MediatR-centric structure.

## What the template already covers
Jason’s template keeps the built-in result type intentionally small so new projects stay approachable:
- `src/Application/Common/Models/Result.cs` exposes `bool Succeeded` plus `string[] Errors`, with static `Success()` and `Failure(IEnumerable<string>)`.
- It is used mainly by the Identity layer (`Infrastructure/Identity/IdentityResultExtensions.cs` converts `IdentityResult` to `Result`; `IdentityService.DeleteUserAsync` returns it). Commands/queries typically return view models or primitives directly, not `Result`.
- MediatR pipeline behaviors (`LoggingBehaviour`, `UnhandledExceptionBehaviour`, `AuthorizationBehaviour`, `ValidationBehaviour`, `PerformanceBehaviour`) assume exceptions for failures (e.g., FluentValidation throws `ValidationException`), not a rich result envelope.
- There is no explicit place for trace/correlation IDs. Logging relies on `ILogger<T>` scopes populated by `IUser`/`IIdentityService` rather than a result metadata contract.
- Errors are unstructured strings; you cannot attach error codes, metadata, or machine-readable details without inventing your own type downstream.

That minimalism works for demos and small features, but it leaves gaps once you need consistent error shapes, correlation-friendly diagnostics, or functional composition across handlers.

## Why use the Core Result primitives
The extension keeps the success/failure semantics but adds the capabilities teams routinely need in production:
- **Traceability:** `Result` and `Error` carry an optional `TraceId` so handlers and web APIs can preserve request/operation identifiers without leaking logging concerns into business code.
- **Structured errors:** `Error` holds `Code`, `Message`, and optional `Metadata` (key/value) to capture domain facts (e.g., constraints, limits, offending values) in a machine-readable way.
- **Value payloads:** `Result<T>` wraps successful values, avoiding parallel DTOs for “maybe failed” responses.
- **Composition:** Helpers like `Map`, `Bind`, `Tap`, `Ensure`, `Recover`, and `Combine` let you chain operations without nested `if` blocks or exception gymnastics.
- **Guard synergy:** Guard clauses in the same package can emit `Result<T>` with trace IDs and error codes, giving you a uniform contract from validation through handler to API.
- **Testability:** Deterministic results and errors make it easy to assert flows without depending on logging side effects or exception throwing.
- **Interop:** You can adapt the template’s `Result` (strings) into the richer `Result`/`Error` shape and back, enabling gradual adoption.

## API surface at a glance
- **Types:** `Result`, `Result<T>`, and `Error` live in `CleanArchitecture.Extensions.Core.Results`.
- **Creation:** `Result.Success(traceId?)`, `Result.Success<T>(value, traceId?)`, `Result.Failure(error|errors, traceId?)`, `Result.Failure<T>(...)`.
- **Inspection:** `IsSuccess`, `IsFailure`, `Errors`, `TraceId`, `Value`, `ValueOrDefault`.
- **Composition:** 
  - `Result<T>.Map(Func<T, TResult>)` and `Bind(Func<T, Result<TResult>>)`
  - `Result<T>.Tap(Action<T>)` for side effects on success
  - `Result<T>.Ensure(predicate, error)` to enforce additional invariants
  - `ResultExtensions.Ensure(predicate, error)` for non-generic results
  - `Result.Combine(params Result[])` to aggregate multiple checks
  - `ResultExtensions.Recover(fallback)` to supply a fallback value on failure
  - `ResultExtensions.ToResult()` to wrap raw values
- **Errors:** `Error` exposes `Code`, `Message`, optional `TraceId`, `Metadata`, `HasMetadata`, and helpers `WithTraceId(...)`, `WithMetadata(key, value)`.

## Compatibility and migration from the template
You can start with the template’s existing patterns and layer Core Results gradually:
- **Mapping template → Core:** `Result.Success()` becomes `Result.Success(traceId)`; `Result.Failure(strings)` can be projected to `Result.Failure(strings.Select(s => new Error("identity", s)))` or use `LegacyResult.Failure(strings).ToResult(traceId)`.
- **Mapping Core → template:** `Result.Success(traceId)` can return `CleanArchitecture.Application.Common.Models.Result.Success()` or `LegacyResult.FromResult(result)`. Failures can flatten via `Errors.Select(e => $"{e.Code}: {e.Message}")` or rely on the adapter’s default formatter.
- **Handlers:** Keep return types as your feature needs (DTOs, primitives). Introduce `Result<T>` where you want richer errors without throwing. You can adopt it per handler; nothing requires a big bang change.
- **Pipelines:** Core Results do not change pipeline signatures; they work with the template’s behaviors. Validation that throws still bubbles through `UnhandledExceptionBehaviour`; you can prefer guard/result composition to avoid exceptions when appropriate.

## Real-world use cases (backed by the sample)

A runnable solution lives at `samples/CleanArchitecture.Extensions.Core.Result.Sample`. It keeps Jason’s `IApplicationDbContext` style while exercising Core Results in a `Projects` feature.

### 1) Creating a project with guards + `Combine`
`samples/CleanArchitecture.Extensions.Core.Result.Sample/src/Application/Projects/Commands/CreateProject/CreateProject.cs`:

```csharp
public async Task<CoreResults.Result<int>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
{
    var traceId = Guid.NewGuid().ToString("N");
    var guardOptions = new GuardOptions { TraceId = traceId };

    var name = Guard.AgainstNullOrWhiteSpace(request.Name, nameof(request.Name), guardOptions)
        .Ensure(n => n.Length <= MaxNameLength, new CoreResults.Error("projects.name.length", $"Project name must be {MaxNameLength} characters or fewer.", traceId));

    var description = Guard.Ensure(request.Description is null || request.Description.Length <= MaxDescriptionLength,
        "projects.description.length",
        $"Description must be {MaxDescriptionLength} characters or fewer.",
        guardOptions);

    var budget = Guard.Ensure(request.Budget >= 0,
        "projects.budget.range",
        "Budget cannot be negative.",
        guardOptions);

    var validation = CoreResults.Result.Combine(name, description, budget);
    if (validation.IsFailure)
    {
        return CoreResults.Result.Failure<int>(validation.Errors, traceId);
    }

    var project = new Project(name.Value, request.Description, request.Budget);

    var duplicateName = await _context.Projects
        .AnyAsync(p => p.Name == project.Name, cancellationToken);

    if (duplicateName)
    {
        var duplicateError = new CoreResults.Error("projects.name.duplicate", "A project with this name already exists.", traceId)
            .WithMetadata("name", project.Name);

        return CoreResults.Result.Failure<int>(duplicateError, traceId);
    }

    _context.Projects.Add(project);
    await _context.SaveChangesAsync(cancellationToken);

    return CoreResults.Result.Success(project.Id, traceId);
}
```
- `GuardOptions.TraceId` seeds correlation on every guard failure and success.
- `Result.Combine` keeps the handler branch-free until all synchronous validations run.
- Errors capture metadata (`name`) before returning `Result<int>` to the caller.

### 2) Closing a project with `Bind` and `Tap`
`samples/CleanArchitecture.Extensions.Core.Result.Sample/src/Application/Projects/Commands/CloseProject/CloseProject.cs`:

```csharp
var closeResult = Guard.AgainstNull(project, nameof(project), guardOptions)
    .Bind(p => EnsureNotClosed(p, traceId))
    .Tap(p => p.Close(DateTimeOffset.UtcNow));

if (closeResult.IsFailure)
{
    return CoreResults.Result.Failure(closeResult.Errors, traceId);
}

await _context.SaveChangesAsync(cancellationToken);

return CoreResults.Result.Success(traceId);

static CoreResults.Result<Project> EnsureNotClosed(Project project, string traceId)
{
    return project.IsClosed
        ? CoreResults.Result.Failure<Project>(new CoreResults.Error("projects.closed", "Project is already closed.", traceId), traceId)
        : CoreResults.Result.Success(project, traceId);
}
```
- `Bind` short-circuits if the entity is missing or already closed.
- `Tap` performs the side effect (marking the project closed) without losing the trace ID.

### 3) Mapping results to HTTP responses
`samples/CleanArchitecture.Extensions.Core.Result.Sample/src/Application/Projects/Queries/GetProjectById/GetProjectById.cs` and `samples/CleanArchitecture.Extensions.Core.Result.Sample/src/Web/Endpoints/Projects.cs`:

```csharp
// Query handler
var projectResult = Guard.AgainstNull(project, nameof(project), guardOptions);

return projectResult.Map(p => new ProjectSummaryDto
{
    Id = p.Id,
    Name = p.Name,
    Budget = p.Budget,
    IsClosed = p.IsClosed,
    ClosedOn = p.ClosedOn
}, traceId);

// Minimal API endpoint
public async Task<IResult> GetProjectById(ISender sender, int id)
{
    var result = await sender.Send(new GetProjectByIdQuery(id));

    return result.Match<IResult>(
        project => TypedResults.Ok(new { project, traceId = result.TraceId }),
        _ => ToProblemResult("Project not found.", result, StatusCodes.Status404NotFound));
}
```
- `Map` preserves the guard trace ID when projecting to a DTO.
- `Match` produces consistent HTTP payloads with structured errors + `traceId`.

## Detailed behavior notes
- **Trace IDs:** If you pass a trace ID into `Result.Success` or `Result.Failure`, it will be copied into contained errors (or derived from the first error when not provided). Combine also prefers the first non-empty trace ID it finds.
- **Error metadata:** Use `WithMetadata` to attach machine-readable context (e.g., offending parameter, limit, current balance). Metadata is left untouched unless you clone the error.
- **Value access:** `Result<T>.Value` throws on failure to prevent accidental use; prefer `ValueOrDefault` when you deliberately ignore failures (rare) or `Match` for explicit branching.
- **Recover:** When you want a fallback without losing errors, `Recover` converts failure into success with a derived value (and carries the original trace ID). Useful for caching or default projections.
- **Thread safety:** Results are immutable; the error list is wrapped in `ReadOnlyCollection<Error>`. You can safely pass results across tasks without extra locking.

## Usage patterns and guidance
- **Prefer `Result<T>` at boundaries:** Commands/queries that are exposed to external callers benefit most because you can return rich, predictable errors to API controllers, background jobs, or message handlers.
- **Keep exceptions for truly exceptional cases:** Domain validation, guard failures, and expected business rule violations should return `Result`; infrastructure failures (DB down, unexpected null) can still throw to be caught by `UnhandledExceptionBehaviour` or global middleware.
- **Name your error codes consistently:** Use domain-prefixed codes (`billing.credit-limit`, `cart.empty`, `user.not-found`) so clients and observability tools can filter/search easily.
- **Propagate correlation:** When you have a correlation ID in `ILogContext` or HTTP headers, pass it into your first `Result` creation so all downstream errors carry it. The pipeline behaviors in Core keep `ILogContext` hydrated.
- **Compose before persisting:** Use `Combine` to gather errors before calling repositories. This keeps persistence logic free of partial states.
- **Testing:** Assert on `IsSuccess/IsFailure`, `Errors.Count`, and `TraceId`. For pipelines, pair `InMemoryAppLogger` with `ILogContext` to assert correlation + timing alongside result flows.

## Frequently asked questions
- **Do I have to return `Result` from every handler?** No. Use it where you want structured errors and correlation in your contract. You can mix `Result<T>` handlers with plain DTO-returning handlers.
- **Will this break the template’s pipeline behaviors?** No. All behaviors keep the same generic signatures. You can register Core behaviors in the same order as the template’s behaviors to preserve semantics.
- **How do I integrate with FluentValidation?** You can keep FluentValidation throwing `ValidationException` (caught by middleware) or map validation failures into `Result` by projecting `ValidationFailure` into `Error` inside handlers/behaviors.
- **Can I log errors automatically?** Yes—hook your logger adapter to inspect `Result` failures and emit logs enriched by `TraceId` and `Metadata`. The Core logging abstractions keep this decoupled from MediatR.

## Step-by-step adoption plan
1) **Add the package** and register Core pipeline behaviors (Correlation, Logging, Performance) plus `IClock`, `ILogContext`, and logger adapters.
2) **Wrap new handlers** that benefit from structured errors in `Result<T>`; start with operations exposed to external clients or sensitive domains (payments, identity).
3) **Introduce guards** as the first layer in handlers to replace ad-hoc `if`/throws. Configure `GuardStrategy` globally via `CoreExtensionsOptions`.
4) **Unify error codes** across teams; codify them in a shared static class or constants to avoid drift.
5) **Bridge Identity/legacy flows** with adapters so you can defer broader refactors while gaining correlation and metadata where you need it.

## Reference: Type summaries
- `Result`: success/failure envelope, trace ID, immutable `IReadOnlyList<Error>`. Factory methods and `Combine`.
- `Result<T>`: extends `Result` with `Value`, `ValueOrDefault`, `Map`, `Bind`, `Tap`, `Ensure`, `Match`, and overrides `Failure` factories to attach trace IDs.
- `Error`: structured error data with helpers to clone metadata or trace IDs; `None` sentinel for convenience.
- `ResultExtensions`: `Ensure` for non-generic results, `ToResult` wrappers, and `Recover`.

## Example test: asserting failures and metadata
```csharp
[Fact]
public void Recover_ReturnsFallbackOnFailure()
{
    var error = new Error("billing.credit-limit", "Insufficient credit", traceId: "req-42");
    var failed = Result.Failure<int>(error);

    var recovered = failed.Recover(errors => errors.Count);

    recovered.IsSuccess.Should().BeTrue();
    recovered.Value.Should().Be(1);
    recovered.TraceId.Should().Be("req-42");
}
```

## Related docs
- [Core extension overview](./core.md) for guard clauses, pipeline behaviors, logging, time, and domain events.
- [Validation extension](./validation.md) when you need FluentValidation-first behaviors that complement or replace guard/result flows.
