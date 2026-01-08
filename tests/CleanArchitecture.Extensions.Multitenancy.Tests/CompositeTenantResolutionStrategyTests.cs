using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using CleanArchitecture.Extensions.Multitenancy.Providers;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.Tests;

public class CompositeTenantResolutionStrategyTests
{
    [Fact]
    public async Task ResolveAsync_uses_configured_order()
    {
        var providers = new ITenantProvider[]
        {
            new StubProvider(TenantResolutionSource.Header, _ =>
                TenantResolutionResult.Resolved("header", TenantResolutionSource.Header)),
            new StubProvider(TenantResolutionSource.Route, _ =>
                TenantResolutionResult.Resolved("route", TenantResolutionSource.Route))
        };

        var options = Options.Create(new MultitenancyOptions
        {
            ResolutionOrder = new List<TenantResolutionSource>
            {
                TenantResolutionSource.Route,
                TenantResolutionSource.Header
            },
            IncludeUnorderedProviders = false
        });

        var strategy = new CompositeTenantResolutionStrategy(providers, options);

        var result = await strategy.ResolveAsync(new TenantResolutionContext());

        Assert.Equal("route", result.TenantId);
        Assert.Equal(TenantResolutionSource.Route, result.Source);
    }

    [Fact]
    public async Task ResolveAsync_returns_ambiguous_when_no_resolution()
    {
        var providers = new ITenantProvider[]
        {
            new StubProvider(TenantResolutionSource.Header, _ =>
                TenantResolutionResult.FromCandidates(new[] { "alpha", "beta" }, TenantResolutionSource.Header)),
            new StubProvider(TenantResolutionSource.Route, _ =>
                TenantResolutionResult.NotFound(TenantResolutionSource.Route))
        };

        var options = Options.Create(new MultitenancyOptions
        {
            ResolutionOrder = new List<TenantResolutionSource>
            {
                TenantResolutionSource.Header,
                TenantResolutionSource.Route
            }
        });

        var strategy = new CompositeTenantResolutionStrategy(providers, options);

        var result = await strategy.ResolveAsync(new TenantResolutionContext());

        Assert.True(result.IsAmbiguous);
        Assert.Equal(TenantResolutionSource.Header, result.Source);
    }

    [Fact]
    public async Task ResolveAsync_consensus_returns_single_candidate()
    {
        var providers = new ITenantProvider[]
        {
            new StubProvider(TenantResolutionSource.Header, _ =>
                TenantResolutionResult.Resolved("tenant-1", TenantResolutionSource.Header)),
            new StubProvider(TenantResolutionSource.Route, _ =>
                TenantResolutionResult.Resolved("tenant-1", TenantResolutionSource.Route))
        };

        var options = Options.Create(new MultitenancyOptions
        {
            RequireMatchAcrossSources = true
        });

        var strategy = new CompositeTenantResolutionStrategy(providers, options);

        var result = await strategy.ResolveAsync(new TenantResolutionContext());

        Assert.Equal("tenant-1", result.TenantId);
        Assert.Equal(TenantResolutionSource.Composite, result.Source);
    }

    [Fact]
    public async Task ResolveAsync_consensus_ignores_default_when_other_candidates_exist()
    {
        var providers = new ITenantProvider[]
        {
            new StubProvider(TenantResolutionSource.Default, _ =>
                TenantResolutionResult.Resolved("fallback", TenantResolutionSource.Default)),
            new StubProvider(TenantResolutionSource.Header, _ =>
                TenantResolutionResult.Resolved("tenant-1", TenantResolutionSource.Header))
        };

        var options = Options.Create(new MultitenancyOptions
        {
            RequireMatchAcrossSources = true
        });

        var strategy = new CompositeTenantResolutionStrategy(providers, options);

        var result = await strategy.ResolveAsync(new TenantResolutionContext());

        Assert.Equal("tenant-1", result.TenantId);
        Assert.Equal(TenantResolutionSource.Composite, result.Source);
    }

    [Fact]
    public async Task ResolveAsync_consensus_uses_default_when_only_source()
    {
        var providers = new ITenantProvider[]
        {
            new StubProvider(TenantResolutionSource.Default, _ =>
                TenantResolutionResult.Resolved("fallback", TenantResolutionSource.Default))
        };

        var options = Options.Create(new MultitenancyOptions
        {
            RequireMatchAcrossSources = true
        });

        var strategy = new CompositeTenantResolutionStrategy(providers, options);

        var result = await strategy.ResolveAsync(new TenantResolutionContext());

        Assert.Equal("fallback", result.TenantId);
        Assert.Equal(TenantResolutionSource.Composite, result.Source);
    }

    private sealed class StubProvider : ITenantProvider
    {
        private readonly Func<TenantResolutionContext, TenantResolutionResult> _resolver;

        public StubProvider(TenantResolutionSource source, Func<TenantResolutionContext, TenantResolutionResult> resolver)
        {
            Source = source;
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        public TenantResolutionSource Source { get; }

        public ValueTask<TenantResolutionResult> ResolveAsync(
            TenantResolutionContext context,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(_resolver(context));
        }
    }
}
