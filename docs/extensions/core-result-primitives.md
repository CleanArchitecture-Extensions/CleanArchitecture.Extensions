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
- **Mapping template → Core:** `Result.Success()` becomes `Result.Success(traceId)`; `Result.Failure(strings)` can be projected to `Result.Failure(strings.Select(s => new Error("identity", s)))`.
- **Mapping Core → template:** `Result.Success(traceId)` can return `CleanArchitecture.Application.Common.Models.Result.Success()`. Failures can flatten via `Errors.Select(e => $"{e.Code}: {e.Message}")`.
- **Handlers:** Keep return types as your feature needs (DTOs, primitives). Introduce `Result<T>` where you want richer errors without throwing. You can adopt it per handler; nothing requires a big bang change.
- **Pipelines:** Core Results do not change pipeline signatures; they work with the template’s behaviors. Validation that throws still bubbles through `UnhandledExceptionBehaviour`; you can prefer guard/result composition to avoid exceptions when appropriate.

## Real-world use cases

### 1) Command input validation with guards and trace IDs
```csharp
public sealed record CreateProjectCommand(string Name, string? Description) : IRequest<Result<Guid>>;

public sealed class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<Guid>>
{
    private readonly IProjectRepository _repository;
    private readonly CoreExtensionsOptions _options;

    public CreateProjectCommandHandler(IProjectRepository repository, IOptions<CoreExtensionsOptions> options)
    {
        _repository = repository;
        _options = options.Value;
    }

    public async Task<Result<Guid>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var guardOptions = GuardOptions.FromOptions(_options);
        var name = Guard.AgainstNullOrWhiteSpace(request.Name, nameof(request.Name), guardOptions);
        if (name.IsFailure) return Result.Failure<Guid>(name.Errors, name.TraceId);

        var project = new Project(name.Value, request.Description);
        await _repository.AddAsync(project, cancellationToken);

        return Result.Success(project.Id, name.TraceId);
    }
}
```
- The handler stays free of exception-heavy flows. Errors carry `TraceId` from options, and the API can forward that to clients.

### 2) Composing multiple operations with `Bind` and `Ensure`
```csharp
public Result<Invoice> TryBillCustomer(Guid customerId, Money amount)
{
    return Guard.AgainstNull(customerId, nameof(customerId))
        .Bind(id => _customerService.GetById(id))
        .Ensure(customer => customer.CreditLimit >= amount, 
            new Error("billing.credit-limit", "Insufficient credit"))
        .Bind(_ => _invoiceService.CreateInvoice(customerId, amount))
        .Tap(invoice => _logger.Info("Issued invoice", new Dictionary<string, object?>
        {
            ["InvoiceId"] = invoice.Id,
            ["CustomerId"] = customerId
        }));
}
```
- `Bind` avoids nested `if` ladders; `Ensure` introduces a new domain check with a domain-specific error code.

### 3) Aggregating results from independent checks
```csharp
public Result ValidateCheckout(Cart cart)
{
    var errors = new List<Result>
    {
        Guard.Ensure(cart.Items.Any(), "cart.empty", "Cart has no items"),
        Guard.Ensure(cart.Total <= cart.Customer.CreditLimit, "cart.limit", "Cart exceeds credit limit"),
        Guard.Ensure(cart.Items.Count <= 100, "cart.too-many-items", "Cart item count too high")
    };

    return Result.Combine(errors);
}
```
- `Result.Combine` aggregates all errors instead of stopping at the first failure, enabling UX that shows all blocking issues at once.

### 4) Adapting to HTTP responses
```csharp
[HttpPost("projects")]
public async Task<IResult> Create([FromServices] IMediator mediator, [FromBody] CreateProjectCommand command)
{
    var result = await mediator.Send(command);

    return result.Match<IResult>(
        onSuccess: id => Results.Created($"/projects/{id}", new { id, traceId = result.TraceId }),
        onFailure: errors =>
        {
            var problem = errors.Select(e => new { e.Code, e.Message, e.TraceId, e.Metadata });
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest,
                detail: "Validation failed",
                extensions: new Dictionary<string, object?>
                {
                    ["errors"] = problem,
                    ["traceId"] = result.TraceId
                });
        });
}
```
- The API keeps the error shape consistent and forwards `TraceId` for client/server troubleshooting.

### 5) Bridging to the template’s Identity Result
```csharp
public static class IdentityResultAdapters
{
    public static Result ToCoreResult(this CleanArchitecture.Application.Common.Models.Result source, string? traceId = null)
    {
        return source.Succeeded
            ? Result.Success(traceId)
            : Result.Failure(source.Errors.Select(e => new Error("identity", e, traceId)));
    }

    public static CleanArchitecture.Application.Common.Models.Result ToTemplateResult(this Result source)
    {
        return source.IsSuccess
            ? CleanArchitecture.Application.Common.Models.Result.Success()
            : CleanArchitecture.Application.Common.Models.Result.Failure(source.Errors.Select(e => $"{e.Code}: {e.Message}"));
    }
}
```
- Use adapters in Identity or integration layers so you can return richer errors to APIs while maintaining existing service contracts elsewhere.

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
