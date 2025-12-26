# CleanArchitecture.Extensions.Caching

Simple setup for the JaysonTaylorCleanArchitectureBlank template.

## Step 1 - Install the package

Install in both Application and Infrastructure projects:

```powershell
dotnet add src/Application/Application.csproj package CleanArchitecture.Extensions.Caching
dotnet add src/Infrastructure/Infrastructure.csproj package CleanArchitecture.Extensions.Caching
```

## Step 2 - Register caching services (Infrastructure layer)

File: `src/Infrastructure/DependencyInjection.cs`

Add the package and register caching:

```csharp
using CleanArchitecture.Extensions.Caching;
using CleanArchitecture.Extensions.Caching.Options;

public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
{
    // existing registrations...

    builder.Services.AddCleanArchitectureCaching(options =>
    {
        options.DefaultNamespace = "MyApp";
        options.MaxEntrySizeBytes = 256 * 1024;
    }, queryOptions =>
    {
        queryOptions.DefaultTtl = TimeSpan.FromMinutes(5);
    });
}
```

## Step 3 - Register the caching behavior (Application layer)

File: `src/Application/DependencyInjection.cs`

Insert the caching behavior after Validation and before Performance:

```csharp
using CleanArchitecture.Extensions.Caching.Behaviors;

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.AddOpenRequestPreProcessor(typeof(LoggingBehaviour<>));
    cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
    cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
    // Add caching behavior below this line ----
    cfg.AddOpenBehavior(typeof(QueryCachingBehavior<,>));
    // Add caching behavior above this line ----
    cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
});
```

## Step 4 - What to expect

- Queries (types ending with `Query`) are cached by default; commands are not.
- First request is a cache miss; the second identical request is a cache hit.
- Default TTL is 5 minutes; override with `QueryCachingBehaviorOptions.DefaultTtl` or `TtlByRequestType`.
- Debug logs show `Cache hit` and `Cache miss` messages when caching is active.
