using System.Diagnostics;
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Behaviors;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using CleanArchitecture.Extensions.Multitenancy.Context;
using CleanArchitecture.Extensions.Multitenancy.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.Tests;

public class TenantEnforcementBehaviorTests
{
    [Fact]
    public async Task Handle_disallows_optional_attribute_when_anonymous_not_allowed()
    {
        var currentTenant = new CurrentTenantAccessor();
        var options = Options.Create(new MultitenancyOptions
        {
            RequireTenantByDefault = true,
            AllowAnonymous = false
        });
        var behavior = new TenantEnforcementBehavior<OptionalAttributeRequest, string>(currentTenant, options);

        await Assert.ThrowsAsync<TenantNotResolvedException>(() =>
            behavior.Handle(new OptionalAttributeRequest(), _ => Task.FromResult("ok"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_allows_optional_attribute_when_anonymous_allowed()
    {
        var currentTenant = new CurrentTenantAccessor();
        var options = Options.Create(new MultitenancyOptions
        {
            RequireTenantByDefault = true,
            AllowAnonymous = true
        });
        var behavior = new TenantEnforcementBehavior<OptionalAttributeRequest, string>(currentTenant, options);

        var response = await behavior.Handle(new OptionalAttributeRequest(), _ => Task.FromResult("ok"), CancellationToken.None);

        Assert.Equal("ok", response);
    }

    [Fact]
    public async Task Handle_requires_tenant_when_attribute_requires()
    {
        var currentTenant = new CurrentTenantAccessor();
        var options = Options.Create(new MultitenancyOptions
        {
            RequireTenantByDefault = false,
            AllowAnonymous = true
        });
        var behavior = new TenantEnforcementBehavior<RequiredAttributeRequest, string>(currentTenant, options);

        await Assert.ThrowsAsync<TenantNotResolvedException>(() =>
            behavior.Handle(new RequiredAttributeRequest(), _ => Task.FromResult("ok"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_throws_when_tenant_not_validated()
    {
        var currentTenant = new CurrentTenantAccessor();
        currentTenant.Current = CreateContext(new TenantInfo("tenant-1"), isValidated: false);
        var options = Options.Create(MultitenancyOptions.Default);
        var behavior = new TenantEnforcementBehavior<DefaultRequest, string>(currentTenant, options);

        await Assert.ThrowsAsync<TenantNotFoundException>(() =>
            behavior.Handle(new DefaultRequest(), _ => Task.FromResult("ok"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_throws_when_tenant_suspended()
    {
        var tenant = new TenantInfo("tenant-1")
        {
            State = TenantState.Suspended
        };
        var currentTenant = new CurrentTenantAccessor
        {
            Current = CreateContext(tenant)
        };
        var options = Options.Create(MultitenancyOptions.Default);
        var behavior = new TenantEnforcementBehavior<DefaultRequest, string>(currentTenant, options);

        await Assert.ThrowsAsync<TenantSuspendedException>(() =>
            behavior.Handle(new DefaultRequest(), _ => Task.FromResult("ok"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_throws_when_tenant_expired()
    {
        var tenant = new TenantInfo("tenant-1")
        {
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1)
        };
        var currentTenant = new CurrentTenantAccessor
        {
            Current = CreateContext(tenant)
        };
        var options = Options.Create(MultitenancyOptions.Default);
        var behavior = new TenantEnforcementBehavior<DefaultRequest, string>(currentTenant, options);

        await Assert.ThrowsAsync<TenantInactiveException>(() =>
            behavior.Handle(new DefaultRequest(), _ => Task.FromResult("ok"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_allows_valid_tenant()
    {
        var tenant = new TenantInfo("tenant-1")
        {
            State = TenantState.Active,
            IsActive = true,
            IsSoftDeleted = false,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5)
        };
        var currentTenant = new CurrentTenantAccessor
        {
            Current = CreateContext(tenant)
        };
        var options = Options.Create(MultitenancyOptions.Default);
        var behavior = new TenantEnforcementBehavior<DefaultRequest, string>(currentTenant, options);

        var response = await behavior.Handle(new DefaultRequest(), _ => Task.FromResult("ok"), CancellationToken.None);

        Assert.Equal("ok", response);
    }

    private static TenantContext CreateContext(TenantInfo tenant, bool isValidated = true)
    {
        var resolution = TenantResolutionResult.Resolved(tenant.TenantId, TenantResolutionSource.Header);
        return new TenantContext(tenant, resolution, isValidated: isValidated);
    }

    private sealed record DefaultRequest : IRequest<string>;

    [AllowHostRequests]
    private sealed record OptionalAttributeRequest : IRequest<string>;

    [RequiresTenant]
    private sealed record RequiredAttributeRequest : IRequest<string>;
}

public class TenantValidationBehaviorTests
{
    [Fact]
    public async Task Handle_skips_when_validation_mode_none()
    {
        var accessor = new CurrentTenantAccessor();
        var tenant = new TenantInfo("tenant-1");
        accessor.Current = new TenantContext(tenant, TenantResolutionResult.Resolved("tenant-1", TenantResolutionSource.Header), isValidated: false);
        var options = Options.Create(new MultitenancyOptions { ValidationMode = TenantValidationMode.None });
        var behavior = new TenantValidationBehavior<DefaultRequest, string>(
            accessor,
            options,
            NullLogger<TenantValidationBehavior<DefaultRequest, string>>.Instance);

        await behavior.Handle(new DefaultRequest(), _ => Task.FromResult("ok"), CancellationToken.None);

        Assert.False(accessor.Current!.IsValidated);
    }

    [Fact]
    public async Task Handle_validates_using_cache()
    {
        var accessor = new CurrentTenantAccessor();
        accessor.Current = new TenantContext(new TenantInfo("tenant-1"), TenantResolutionResult.Resolved("tenant-1", TenantResolutionSource.Header), isValidated: false);
        var cache = new StubTenantInfoCache(_ => new TenantInfo("tenant-1") { Name = "Cached" });
        var options = Options.Create(new MultitenancyOptions { ValidationMode = TenantValidationMode.Cache });
        var behavior = new TenantValidationBehavior<DefaultRequest, string>(
            accessor,
            options,
            NullLogger<TenantValidationBehavior<DefaultRequest, string>>.Instance,
            tenantCache: cache);

        await behavior.Handle(new DefaultRequest(), _ => Task.FromResult("ok"), CancellationToken.None);

        Assert.True(accessor.Current!.IsValidated);
        Assert.Equal("Cached", accessor.Current.Tenant.Name);
        Assert.Equal(1, cache.GetCallCount);
    }

    [Fact]
    public async Task Handle_validates_using_repository_and_sets_cache()
    {
        var accessor = new CurrentTenantAccessor();
        accessor.Current = new TenantContext(new TenantInfo("tenant-1"), TenantResolutionResult.Resolved("tenant-1", TenantResolutionSource.Header), isValidated: false);
        var store = new StubTenantInfoStore(_ => new TenantInfo("tenant-1") { Name = "Stored" });
        var cache = new StubTenantInfoCache(_ => null);
        var options = Options.Create(new MultitenancyOptions { ValidationMode = TenantValidationMode.Repository });
        var behavior = new TenantValidationBehavior<DefaultRequest, string>(
            accessor,
            options,
            NullLogger<TenantValidationBehavior<DefaultRequest, string>>.Instance,
            tenantStore: store,
            tenantCache: cache);

        await behavior.Handle(new DefaultRequest(), _ => Task.FromResult("ok"), CancellationToken.None);

        Assert.True(accessor.Current!.IsValidated);
        Assert.Equal("Stored", accessor.Current.Tenant.Name);
        Assert.Equal(1, store.CallCount);
        Assert.Equal(1, cache.SetCallCount);
    }

    private sealed record DefaultRequest : IRequest<string>;
}

public class TenantCorrelationBehaviorTests
{
    [Fact]
    public async Task Handle_sets_activity_when_log_scope_disabled()
    {
        var currentTenant = new CurrentTenantAccessor
        {
            Current = new TenantContext(new TenantInfo("tenant-1"), TenantResolutionResult.Resolved("tenant-1", TenantResolutionSource.Header))
        };
        var options = Options.Create(new MultitenancyOptions
        {
            AddTenantToLogScope = false,
            AddTenantToActivity = true
        });
        var scopeAccessor = new TenantCorrelationScopeAccessor();
        var behavior = new TenantCorrelationBehavior<DefaultRequest, string>(
            currentTenant,
            options,
            NullLogger<TenantCorrelationBehavior<DefaultRequest, string>>.Instance,
            scopeAccessor);
        var activity = new Activity("test");
        activity.Start();

        await behavior.Handle(new DefaultRequest(), _ => Task.FromResult("ok"), CancellationToken.None);

        Assert.Equal("tenant-1", activity.GetBaggageItem("tenant_id"));
        Assert.Equal("tenant-1", activity.GetTagItem("tenant_id"));
        activity.Stop();
    }

    [Fact]
    public async Task Handle_sets_activity_baggage_when_enabled()
    {
        var currentTenant = new CurrentTenantAccessor
        {
            Current = new TenantContext(new TenantInfo("tenant-1"), TenantResolutionResult.Resolved("tenant-1", TenantResolutionSource.Header))
        };
        var options = Options.Create(new MultitenancyOptions
        {
            AddTenantToLogScope = true,
            AddTenantToActivity = true,
            LogScopeKey = "tenant"
        });
        var scopeAccessor = new TenantCorrelationScopeAccessor();
        var behavior = new TenantCorrelationBehavior<DefaultRequest, string>(
            currentTenant,
            options,
            NullLogger<TenantCorrelationBehavior<DefaultRequest, string>>.Instance,
            scopeAccessor);
        var activity = new Activity("test");
        activity.Start();

        await behavior.Handle(new DefaultRequest(), _ => Task.FromResult("ok"), CancellationToken.None);

        Assert.Equal("tenant-1", activity.GetBaggageItem("tenant"));
        Assert.Equal("tenant-1", activity.GetTagItem("tenant"));
        activity.Stop();
    }

    private sealed record DefaultRequest : IRequest<string>;
}
