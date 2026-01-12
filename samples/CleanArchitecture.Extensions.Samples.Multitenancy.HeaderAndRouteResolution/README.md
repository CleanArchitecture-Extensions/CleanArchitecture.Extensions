# CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution

The project was generated using the [Clean.Architecture.Solution.Template](https://github.com/jasontaylordev/CleanArchitecture) version 10.0.0-preview.

## Sample Steps

### Step 1: Create the base solution
Generate the empty Web API-only solution with SQLite using the Clean Architecture template. This matches the baseline template so the multitenancy changes are easy to compare and repeat.

### Step 2: Add multitenancy NuGet packages
Reference the published Multitenancy packages from NuGet (use the latest versions available).

Packages:
- [CleanArchitecture.Extensions.Multitenancy](https://www.nuget.org/packages/CleanArchitecture.Extensions.Multitenancy)
- [CleanArchitecture.Extensions.Multitenancy.AspNetCore](https://www.nuget.org/packages/CleanArchitecture.Extensions.Multitenancy.AspNetCore)

`samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution/src/Application/Application.csproj`:
```xml
<!-- Step 2: (Begin) Add Multitenancy core package -->
<PackageReference Include="CleanArchitecture.Extensions.Multitenancy" VersionOverride="0.2.6" />
<!-- Step 2: (End) Add Multitenancy core package -->
```

`samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution/src/Web/Web.csproj`:
```xml
<!-- Step 2: (Begin) Add Multitenancy AspNetCore package -->
<PackageReference Include="CleanArchitecture.Extensions.Multitenancy.AspNetCore" VersionOverride="0.2.6" />
<!-- Step 2: (End) Add Multitenancy AspNetCore package -->
```

### Step 3: Configure multitenancy resolution defaults
Set route-first ordering, require tenants by default, and disable fallback tenants.

`samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution/src/Web/DependencyInjection.cs`:
```csharp
// Step 3: (Begin) Multitenancy configuration imports
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
// Step 3: (End) Multitenancy configuration imports
```
```csharp
// Step 3: (Begin) Configure multitenancy resolution defaults
builder.Services.Configure<MultitenancyOptions>(options =>
{
    options.RequireTenantByDefault = true;
    options.HeaderNames = new[] { "X-Tenant-ID" };
    options.ResolutionOrder = new List<TenantResolutionSource>
    {
        TenantResolutionSource.Route,
        TenantResolutionSource.Host,
        TenantResolutionSource.Header,
        TenantResolutionSource.QueryString,
        TenantResolutionSource.Claim
    };
    options.FallbackTenant = null;
    options.FallbackTenantId = null;
});
// Step 3: (End) Configure multitenancy resolution defaults
```

### Step 4: Register multitenancy services and middleware
Register the core and ASP.NET Core services, then add the middleware between routing and auth.

`samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution/src/Web/DependencyInjection.cs`:
```csharp
// Step 4: (Begin) Multitenancy ASP.NET Core registration imports
using CleanArchitecture.Extensions.Multitenancy.AspNetCore;
// Step 4: (End) Multitenancy ASP.NET Core registration imports
```
```csharp
// Step 4: (Begin) Register multitenancy services and ASP.NET Core adapter
builder.Services.AddCleanArchitectureMultitenancy();
builder.Services.AddCleanArchitectureMultitenancyAspNetCore(autoUseMiddleware: false);
// Step 4: (End) Register multitenancy services and ASP.NET Core adapter
```

`samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution/src/Web/Program.cs`:
```csharp
// Step 4: (Begin) Multitenancy middleware import
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Middleware;
// Step 4: (End) Multitenancy middleware import
```
```csharp
// Step 4: (Begin) Add multitenancy middleware between routing and auth
app.UseRouting();
app.UseCleanArchitectureMultitenancy();
app.UseAuthentication();
app.UseAuthorization();
// Step 4: (End) Add multitenancy middleware between routing and auth
```

### Step 5: Add tenant route prefix for endpoint groups
Group tenant-bound APIs under `/api/tenants/{tenantId}/...`. Health and status endpoints remain outside this group.

`samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution/src/Web/Infrastructure/WebApplicationExtensions.cs`:
```csharp
// Step 5: (Begin) Prefix tenant-bound endpoints with tenant route
var tenantRoutePrefix = "/api/tenants/{tenantId}";

return app
    .MapGroup($"{tenantRoutePrefix}/{groupName}")
    .WithGroupName(groupName)
    .WithTags(groupName);
// Step 5: (End) Prefix tenant-bound endpoints with tenant route
```

## Build

Run `dotnet build -tl` to build the solution.

## Run

To run the web application:

```bash
cd .\src\Web\
dotnet watch run
```

Navigate to https://localhost:5001. The application will automatically reload if you change any of the source files.

## Code Styles & Formatting

The template includes [EditorConfig](https://editorconfig.org/) support to help maintain consistent coding styles for multiple developers working on the same project across various editors and IDEs. The **.editorconfig** file defines the coding styles applicable to this solution.

## Code Scaffolding

The template includes support to scaffold new commands and queries.

Start in the `.\src\Application\` folder.

Create a new command:

```
dotnet new ca-usecase --name CreateTodoList --feature-name TodoLists --usecase-type command --return-type int
```

Create a new query:

```
dotnet new ca-usecase -n GetTodos -fn TodoLists -ut query -rt TodosVm
```

If you encounter the error *"No templates or subcommands found matching: 'ca-usecase'."*, install the template and try again:

```bash
dotnet new install Clean.Architecture.Solution.Template::10.0.0-preview
```

## Test

The solution contains unit, integration, and functional tests.

To run the tests:
```bash
dotnet test
```

## Help
To learn more about the template go to the [project website](https://github.com/jasontaylordev/CleanArchitecture). Here you can find additional guidance, request new features, report a bug, and discuss the template with other users.
