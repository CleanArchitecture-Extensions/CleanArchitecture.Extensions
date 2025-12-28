using CleanArchitecture.Extensions.Multitenancy.Abstractions;

namespace CleanArchitecture.Extensions.Multitenancy.Tests;

internal sealed class StubTenantInfoStore : ITenantInfoStore
{
    private readonly Func<string, ITenantInfo?> _resolver;

    public StubTenantInfoStore(Func<string, ITenantInfo?> resolver)
    {
        _resolver = resolver;
    }

    public int CallCount { get; private set; }

    public Task<ITenantInfo?> FindByIdAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        CallCount++;
        return Task.FromResult(_resolver(tenantId));
    }
}

internal sealed class StubTenantInfoCache : ITenantInfoCache
{
    private readonly Func<string, ITenantInfo?> _resolver;

    public StubTenantInfoCache(Func<string, ITenantInfo?> resolver)
    {
        _resolver = resolver;
    }

    public int GetCallCount { get; private set; }

    public int SetCallCount { get; private set; }

    public ITenantInfo? LastSetTenant { get; private set; }

    public TimeSpan? LastTtl { get; private set; }

    public Task<ITenantInfo?> GetAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        GetCallCount++;
        return Task.FromResult(_resolver(tenantId));
    }

    public Task SetAsync(ITenantInfo tenant, TimeSpan? ttl, CancellationToken cancellationToken = default)
    {
        SetCallCount++;
        LastSetTenant = tenant;
        LastTtl = ttl;
        return Task.CompletedTask;
    }
}
