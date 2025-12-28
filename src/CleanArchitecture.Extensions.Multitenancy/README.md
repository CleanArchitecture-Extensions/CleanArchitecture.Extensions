# CleanArchitecture.Extensions.Multitenancy

Core multitenancy primitives and MediatR behaviors for the JasonTaylorCleanArchitectureBlank template.

## Step 1 - Install the package

Install in both Application and Infrastructure projects:

```powershell
dotnet add src/Application/Application.csproj package CleanArchitecture.Extensions.Multitenancy
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Multitenancy
```

## Step 2 - Register multitenancy services (Infrastructure layer)

File: `src/Infrastructure/DependencyInjection.cs`

```csharp
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Configuration;

public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
{
    // existing registrations...

    builder.Services.AddCleanArchitectureMultitenancy(options =>
    {
        options.HeaderNames = new[] { "X-Tenant-ID" };
        options.RouteParameterName = "tenantId";
        options.QueryParameterName = "tenantId";
        options.ClaimType = "tenant_id";
    });
}
```

Optional: when using caching, register the tenant-aware cache scope after caching services:

```csharp
builder.Services.AddCleanArchitectureCaching();
builder.Services.AddCleanArchitectureMultitenancyCaching();
```

## Step 3 - Register multitenancy behaviors (Application layer)

File: `src/Application/DependencyInjection.cs`

```csharp
using CleanArchitecture.Extensions.Multitenancy.Behaviors;

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.AddOpenRequestPreProcessor(typeof(LoggingBehaviour<>));
    cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
    cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
    // Add multitenancy behaviors below this line ----
    cfg.AddOpenBehavior(typeof(TenantValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(TenantEnforcementBehavior<,>));
    cfg.AddOpenBehavior(typeof(TenantCorrelationBehavior<,>));
    // Add multitenancy behaviors above this line ----
    cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
});
```

## Step 4 - Use tenant context

The core package stores tenant context in an AsyncLocal accessor. Web-specific resolution is provided by the
`CleanArchitecture.Extensions.Multitenancy.AspNetCore` package; background workers can set the context manually:

```csharp
using CleanArchitecture.Extensions.Multitenancy.Abstractions;

public class TenantJob
{
    private readonly ITenantAccessor _tenantAccessor;

    public TenantJob(ITenantAccessor tenantAccessor)
    {
        _tenantAccessor = tenantAccessor;
    }

    public Task ExecuteAsync()
    {
        var tenant = new TenantInfo("tenant-1");
        using var scope = _tenantAccessor.BeginScope(new TenantContext(tenant, TenantResolutionResult.Resolved("tenant-1", TenantResolutionSource.Custom)));
        // do work within tenant boundary
        return Task.CompletedTask;
    }
}
```
