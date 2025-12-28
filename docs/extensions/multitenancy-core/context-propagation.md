# Multitenancy Core: Context propagation

Tenant context is stored in `AsyncLocal` via `CurrentTenantAccessor`. This page shows how to set and propagate tenant context safely.

## BeginScope for background work

Use `ITenantAccessor.BeginScope` to ensure the previous context is restored:

```csharp
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;

public sealed class TenantJob
{
    private readonly ITenantAccessor _accessor;

    public TenantJob(ITenantAccessor accessor)
    {
        _accessor = accessor;
    }

    public Task ExecuteAsync(string tenantId, CancellationToken cancellationToken)
    {
        var tenant = new TenantInfo(tenantId);
        var resolution = TenantResolutionResult.Resolved(tenantId, TenantResolutionSource.Custom);
        using var scope = _accessor.BeginScope(new TenantContext(tenant, resolution));

        // perform tenant-bound work
        return Task.CompletedTask;
    }
}
```

## Serialize context for jobs and messages

`ITenantContextSerializer` lets you store the full context in job metadata or message headers:

```csharp
using CleanArchitecture.Extensions.Multitenancy.Abstractions;

public sealed class TenantMessagePublisher
{
    private readonly ITenantContextSerializer _serializer;
    private readonly ICurrentTenant _currentTenant;

    public TenantMessagePublisher(ITenantContextSerializer serializer, ICurrentTenant currentTenant)
    {
        _serializer = serializer;
        _currentTenant = currentTenant;
    }

    public string CreateHeader()
    {
        var context = _currentTenant.Context;
        return context is null ? string.Empty : _serializer.Serialize(context);
    }
}
```

On the consumer side:

```csharp
using CleanArchitecture.Extensions.Multitenancy.Abstractions;

public sealed class TenantMessageHandler
{
    private readonly ITenantAccessor _accessor;
    private readonly ITenantContextSerializer _serializer;

    public TenantMessageHandler(ITenantAccessor accessor, ITenantContextSerializer serializer)
    {
        _accessor = accessor;
        _serializer = serializer;
    }

    public Task HandleAsync(string header)
    {
        var context = string.IsNullOrWhiteSpace(header) ? null : _serializer.Deserialize(header);
        using var scope = _accessor.BeginScope(context);

        // handle message
        return Task.CompletedTask;
    }
}
```

## AsyncLocal guidance

- Always wrap work in `BeginScope` so contexts are restored.
- Avoid storing `TenantContext` in static fields.
- Restore context in background workers and message handlers explicitly.
