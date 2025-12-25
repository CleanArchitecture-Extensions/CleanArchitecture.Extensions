# Quickstart

Get caching into a template-based solution in a few minutes.

1. Install the package:
   ```powershell
   dotnet add package CleanArchitecture.Extensions.Caching
   ```
2. Register caching and the MediatR pipeline behavior:
   ```csharp
   services.AddCleanArchitectureCaching();
   services.AddMediatR(cfg => cfg.AddCleanArchitectureCachingPipeline());
   ```
3. Configure cache options (expiration, predicate) as needed.
4. Keep validation and exception handling on the template defaults.

Next: [Caching docs](../extensions/caching.md).
