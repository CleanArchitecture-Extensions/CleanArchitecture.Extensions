using CleanArchitecture.Extensions.Caching.Abstractions;
using CleanArchitecture.Extensions.Caching.Keys;
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Behaviors;
using CleanArchitecture.Extensions.Multitenancy.Context;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;

namespace CleanArchitecture.Extensions.Multitenancy.Caching.Tests;

public class TenantScopedCacheBehaviorTests
{
    [Fact]
    public async Task Handle_returns_next_result()
    {
        var currentTenant = new CurrentTenantAccessor
        {
            Current = new TenantContext(new TenantInfo("tenant-1"), TenantResolutionResult.Resolved("tenant-1", TenantResolutionSource.Header))
        };
        var cacheScope = new StubCacheScope("tenant-2");
        var behavior = new TenantScopedCacheBehavior<DefaultRequest, string>(
            currentTenant,
            cacheScope,
            NullLogger<TenantScopedCacheBehavior<DefaultRequest, string>>.Instance);

        var response = await behavior.Handle(new DefaultRequest(), _ => Task.FromResult("ok"), CancellationToken.None);

        Assert.Equal("ok", response);
    }

    private sealed record DefaultRequest : IRequest<string>;

    private sealed class StubCacheScope : ICacheScope
    {
        public StubCacheScope(string? tenantId)
        {
            TenantId = tenantId;
        }

        public string Namespace => "tests";

        public string? TenantId { get; }

        public CacheKey Create(string resource, string hash) => new(Namespace, resource, hash, TenantId);

        public CacheKey CreateForRequest<TRequest>(string hash) => Create(typeof(TRequest).Name, hash);
    }
}
