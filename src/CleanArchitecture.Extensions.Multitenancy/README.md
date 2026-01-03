# CleanArchitecture.Extensions.Multitenancy

Core multitenancy primitives and MediatR behaviors for Jason Taylor's Clean Architecture template. This package is host-agnostic: it does not depend on ASP.NET Core or EF Core. You supply the host adapter that builds a `TenantResolutionContext` and sets the current tenant.

## What you get

- Tenant model (`TenantInfo`), context (`TenantContext`), and resolution metadata.
- Resolution pipeline with built-in providers (route/host/header/query/claim/default) and customizable ordering.
- Validation hooks (`ITenantInfoStore`, `ITenantInfoCache`) with cache-or-repository validation.
- MediatR behaviors for validation, enforcement, correlation, and cache scope alignment.
- AsyncLocal current-tenant accessor and JSON serializer for background jobs and messages.

## Install

```powershell
dotnet add src/Application/Application.csproj package CleanArchitecture.Extensions.Multitenancy
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Multitenancy
```

Optional caching integration (requires `CleanArchitecture.Extensions.Caching` and `CleanArchitecture.Extensions.Multitenancy.Caching`):

```powershell
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Caching
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Multitenancy.Caching
```

## Quickstart

### 1) Register multitenancy services

```csharp
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Configuration;

builder.Services.AddCleanArchitectureMultitenancy(options =>
{
    options.HeaderNames = new[] { "X-Tenant-ID" };
    options.RouteParameterName = "tenantId";
    options.QueryParameterName = "tenantId";
    options.ClaimType = "tenant_id";

    // Optional defaults
    // options.FallbackTenantId = "local";
    // options.ValidationMode = TenantValidationMode.Repository;
});
```

### 2) Resolve a tenant in your host and set the current context

The core package is host-agnostic. Populate a `TenantResolutionContext` from your environment and call `ITenantResolver`. Example: minimal ASP.NET Core middleware:

```csharp
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;

public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(
        HttpContext httpContext,
        ITenantResolver tenantResolver,
        ITenantAccessor tenantAccessor)
    {
        var resolution = new TenantResolutionContext
        {
            Host = httpContext.Request.Host.Host,
            CorrelationId = httpContext.TraceIdentifier
        };

        foreach (var header in httpContext.Request.Headers)
        {
            resolution.Headers[header.Key] = header.Value.ToString();
        }

        foreach (var route in httpContext.Request.RouteValues)
        {
            if (route.Value is not null)
            {
                resolution.RouteValues[route.Key] = route.Value.ToString()!;
            }
        }

        foreach (var query in httpContext.Request.Query)
        {
            resolution.Query[query.Key] = query.Value.ToString();
        }

        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            foreach (var claim in httpContext.User.Claims)
            {
                resolution.Claims[claim.Type] = claim.Value;
            }
        }

        var tenantContext = await tenantResolver.ResolveAsync(resolution, httpContext.RequestAborted);

        using (tenantAccessor.BeginScope(tenantContext))
        {
            await _next(httpContext);
        }
    }
}
```

### 3) Add multitenancy pipeline behaviors

```csharp
using CleanArchitecture.Extensions.Multitenancy.Behaviors;
using MediatR;

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();

    // Existing behaviors...
    // cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
    // cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));

    cfg.AddCleanArchitectureMultitenancyPipeline();

    // Optional: warn when cache scope is missing tenant context (Multitenancy.Caching package)
    cfg.AddOpenBehavior(typeof(TenantScopedCacheBehavior<,>));
});
```

In the Jason Taylor template, keep the multitenancy pipeline after authorization behaviors so authorization runs first.

### 4) Use tenant context in handlers or jobs

```csharp
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;

public sealed class GetTenantSummaryHandler
{
    private readonly ICurrentTenant _currentTenant;

    public GetTenantSummaryHandler(ICurrentTenant currentTenant)
    {
        _currentTenant = currentTenant;
    }

    public string Handle()
    {
        return _currentTenant.TenantId ?? "no-tenant";
    }
}
```

## Configuration highlights

`MultitenancyOptions` controls resolution order, defaults, and validation behavior:

```csharp
builder.Services.Configure<MultitenancyOptions>(options =>
{
    options.RequireTenantByDefault = true;
    options.AllowAnonymous = false;
    options.ResolutionOrder = new List<TenantResolutionSource>
    {
        TenantResolutionSource.Route,
        TenantResolutionSource.Host,
        TenantResolutionSource.Header,
        TenantResolutionSource.QueryString,
        TenantResolutionSource.Claim,
        TenantResolutionSource.Default
    };

    options.RequireMatchAcrossSources = false;
    options.IncludeUnorderedProviders = true;
    options.ResolutionCacheTtl = TimeSpan.FromMinutes(5);
    options.AddTenantToLogScope = true;
    options.AddTenantToActivity = true;
});
```

Key defaults (all configurable):
- Header name: `X-Tenant-ID`
- Route/query parameter: `tenantId`
- Claim type: `tenant_id`
- Resolution order: Route > Host > Header > Query > Claim > Default
- Validation mode: `None`

## Providers and resolution order

Built-in providers:
- `RouteTenantProvider`, `HostTenantProvider`, `HeaderTenantProvider`, `QueryTenantProvider`, `ClaimTenantProvider`, `DefaultTenantProvider`.

Notes:
- Header/query/claim values can contain multiple candidates separated by `,` or `;`. Multiple candidates are treated as ambiguous and will not resolve a tenant.
- `HostTenantProvider` defaults to the first subdomain (for example, `tenant.app.com` -> `tenant`); set `HostTenantSelector` to customize.
- `RequireMatchAcrossSources` collects candidates from all providers and resolves only if there is exactly one unique candidate.

Custom provider example:

```csharp
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Providers;

builder.Services.AddSingleton<ITenantProvider>(
    new DelegateTenantProvider(context =>
    {
        if (context.Items.TryGetValue("tenant", out var value))
        {
            return value?.ToString();
        }

        return null;
    }));
```

## Validation and lifecycle enforcement

Validation is optional but recommended to prevent spoofed tenant IDs.

```csharp
builder.Services.Configure<MultitenancyOptions>(options =>
{
    options.ValidationMode = TenantValidationMode.Repository;
    options.ResolutionCacheTtl = TimeSpan.FromMinutes(10);
});
```

Implement `ITenantInfoStore` (and optionally `ITenantInfoCache`):

```csharp
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;

public sealed class InMemoryTenantStore : ITenantInfoStore
{
    private readonly Dictionary<string, ITenantInfo> _tenants =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["tenant-1"] = new TenantInfo("tenant-1") { Name = "Tenant One", IsActive = true }
        };

    public Task<ITenantInfo?> FindByIdAsync(string tenantId, CancellationToken cancellationToken = default)
        => Task.FromResult(_tenants.TryGetValue(tenantId, out var tenant) ? tenant : null);
}
```

`TenantEnforcementBehavior` throws when:
- No tenant is resolved (`TenantNotResolvedException`).
- Tenant is not validated (`TenantNotFoundException`).
- Tenant is suspended, inactive, soft-deleted, pending provision, or expired.

Use `AllowHostRequestsAttribute` or `ITenantRequirement` to mark optional endpoints/requests.

## Caching integration

When using `CleanArchitecture.Extensions.Caching`, install the multitenancy caching adapter so cache keys include `tenantId`:

```powershell
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Multitenancy.Caching
```

```csharp
using CleanArchitecture.Extensions.Caching;
using CleanArchitecture.Extensions.Multitenancy;

builder.Services.AddCleanArchitectureCaching();
builder.Services.AddCleanArchitectureMultitenancyCaching();
```

Add the cache scope warning behavior if desired:

```csharp
cfg.AddOpenBehavior(typeof(TenantScopedCacheBehavior<,>));
```

## Background jobs and messaging

Use `ITenantContextSerializer` to flow tenant context through job payloads or message headers:

```csharp
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;

public sealed class TenantJob
{
    private readonly ITenantAccessor _tenantAccessor;
    private readonly ITenantContextSerializer _serializer;

    public TenantJob(ITenantAccessor tenantAccessor, ITenantContextSerializer serializer)
    {
        _tenantAccessor = tenantAccessor;
        _serializer = serializer;
    }

    public Task EnqueueAsync(TenantContext context)
    {
        var payload = _serializer.Serialize(context);
        // store payload in job metadata
        return Task.CompletedTask;
    }

    public Task ExecuteAsync(string payload)
    {
        var context = _serializer.Deserialize(payload);
        using var scope = _tenantAccessor.BeginScope(context);
        // work within tenant boundary
        return Task.CompletedTask;
    }
}
```

## Exceptions

- `TenantNotResolvedException` - no tenant resolved when required.
- `TenantNotFoundException` - tenant ID could not be validated.
- `TenantInactiveException` - tenant is inactive, expired, deleted, or soft-deleted.
- `TenantSuspendedException` - tenant is suspended.

## Related modules (planned)

- `CleanArchitecture.Extensions.Multitenancy.AspNetCore`
- `CleanArchitecture.Extensions.Multitenancy.EFCore`
- `CleanArchitecture.Extensions.Multitenancy.Identity`
- `CleanArchitecture.Extensions.Multitenancy.Provisioning`
- `CleanArchitecture.Extensions.Multitenancy.Redis`
- `CleanArchitecture.Extensions.Multitenancy.Sharding`
- `CleanArchitecture.Extensions.Multitenancy.Storage`

## More documentation

See the deep-dive docs under `CleanArchitecture.Extensions/docs/extensions/multitenancy-core.md`.
