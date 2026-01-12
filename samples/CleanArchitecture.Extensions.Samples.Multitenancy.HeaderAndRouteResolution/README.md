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

The sample uses central package management, so versions live in `Directory.Packages.props`. You can use `dotnet add package` or edit the files directly as shown below.

`samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution/Directory.Packages.props`:
```xml
<!-- Step 2: (Begin) Pin CleanArchitecture.Extensions package version -->
<CleanArchitectureExtensionsVersion>0.1.1-preview.1</CleanArchitectureExtensionsVersion>
<!-- Step 2: (End) Pin CleanArchitecture.Extensions package version -->
```
```xml
<!-- Step 2: (Begin) Add Multitenancy package versions -->
<PackageVersion Include="CleanArchitecture.Extensions.Multitenancy" Version="$(CleanArchitectureExtensionsVersion)" />
<PackageVersion Include="CleanArchitecture.Extensions.Multitenancy.AspNetCore" Version="$(CleanArchitectureExtensionsVersion)" />
<!-- Step 2: (End) Add Multitenancy package versions -->
```

`samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution/src/Application/Application.csproj`:
```xml
<!-- Step 2: (Begin) Add Multitenancy core package -->
<PackageReference Include="CleanArchitecture.Extensions.Multitenancy" />
<!-- Step 2: (End) Add Multitenancy core package -->
```

`samples/CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution/src/Web/Web.csproj`:
```xml
<!-- Step 2: (Begin) Add Multitenancy AspNetCore package -->
<PackageReference Include="CleanArchitecture.Extensions.Multitenancy.AspNetCore" />
<!-- Step 2: (End) Add Multitenancy AspNetCore package -->
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
