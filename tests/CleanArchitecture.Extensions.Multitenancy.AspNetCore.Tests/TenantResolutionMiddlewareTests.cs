using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Context;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Middleware;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Options;
using CleanArchitecture.Extensions.Multitenancy.Behaviors;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using CleanArchitecture.Extensions.Multitenancy.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Tests;

public class TenantResolutionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_sets_current_tenant_and_http_context_items()
    {
        var accessor = new CurrentTenantAccessor();
        var resolver = new CapturingTenantResolver();
        var aspNetOptions = OptionsFactory.Create(new AspNetCoreMultitenancyOptions());
        var coreOptions = OptionsFactory.Create(new MultitenancyOptions());
        var factory = new DefaultTenantResolutionContextFactory(aspNetOptions);
        var logger = NullLogger<TenantResolutionMiddleware>.Instance;

        string? observedTenantId = null;
        TenantContext? observedContext = null;

        RequestDelegate next = ctx =>
        {
            observedTenantId = accessor.TenantId;
            observedContext = ctx.Items[aspNetOptions.Value.HttpContextItemKey] as TenantContext;
            return Task.CompletedTask;
        };

        var middleware = new TenantResolutionMiddleware(next);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Tenant-ID"] = "tenant-1";

        await middleware.InvokeAsync(
            httpContext,
            resolver,
            accessor,
            factory,
            aspNetOptions,
            coreOptions,
            logger,
            new TenantCorrelationScopeAccessor());

        Assert.Equal("tenant-1", observedTenantId);
        Assert.NotNull(observedContext);
        Assert.Null(accessor.TenantId);
    }

    [Fact]
    public void UseCleanArchitectureMultitenancy_builds_pipeline_with_validated_scopes()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCleanArchitectureMultitenancyAspNetCore();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true
        });

        var app = new ApplicationBuilder(provider);
        app.UseCleanArchitectureMultitenancy();
        app.Run(_ => Task.CompletedTask);

        var exception = Record.Exception(() => app.Build());
        Assert.Null(exception);
    }

    private sealed class CapturingTenantResolver : ITenantResolver
    {
        public TenantResolutionContext? CapturedContext { get; private set; }

        public Task<TenantContext?> ResolveAsync(TenantResolutionContext context, CancellationToken cancellationToken = default)
        {
            CapturedContext = context;
            var tenantInfo = new TenantInfo("tenant-1") { IsActive = true, State = TenantState.Active };
            var resolution = TenantResolutionResult.Resolved("tenant-1", TenantResolutionSource.Header);
            return Task.FromResult<TenantContext?>(new TenantContext(tenantInfo, resolution));
        }
    }
}
