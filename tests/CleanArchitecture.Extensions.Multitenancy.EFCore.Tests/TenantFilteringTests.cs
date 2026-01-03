using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Context;
using CleanArchitecture.Extensions.Multitenancy.EFCore;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Filters;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Interceptors;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;
using CleanArchitecture.Extensions.Multitenancy.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Tests;

public class TenantFilteringTests
{
    [Fact]
    public void Query_filter_returns_current_tenant_only()
    {
        var currentTenant = new CurrentTenantAccessor
        {
            Current = CreateTenantContext("tenant-1")
        };
        var options = new EfCoreMultitenancyOptions
        {
            Mode = TenantIsolationMode.SharedDatabase,
            EnableQueryFilters = true,
            UseShadowTenantId = false,
            EnableSaveChangesEnforcement = false
        };

        using var context = CreateContext(currentTenant, options);

        context.Records.AddRange(
            new TenantRecord { TenantId = "tenant-1", Name = "One" },
            new TenantRecord { TenantId = "tenant-2", Name = "Two" });

        context.SaveChanges();

        var results = context.Records.ToList();

        Assert.Single(results);
        Assert.Equal("tenant-1", results[0].TenantId);
    }

    [Fact]
    public void SaveChanges_sets_tenant_id_for_added_entities()
    {
        var currentTenant = new CurrentTenantAccessor
        {
            Current = CreateTenantContext("tenant-1")
        };
        var options = new EfCoreMultitenancyOptions
        {
            Mode = TenantIsolationMode.SharedDatabase,
            EnableSaveChangesEnforcement = true,
            UseShadowTenantId = false
        };

        using var context = CreateContext(currentTenant, options, useInterceptor: true);

        var record = new TenantRecord { Name = "New" };
        context.Records.Add(record);
        context.SaveChanges();

        Assert.Equal("tenant-1", record.TenantId);
    }

    [Fact]
    public void SaveChanges_throws_when_tenant_missing()
    {
        var currentTenant = new CurrentTenantAccessor();
        var options = new EfCoreMultitenancyOptions
        {
            Mode = TenantIsolationMode.SharedDatabase,
            EnableSaveChangesEnforcement = true,
            RequireTenantForWrites = true,
            UseShadowTenantId = false
        };

        using var context = CreateContext(currentTenant, options, useInterceptor: true);

        context.Records.Add(new TenantRecord { Name = "New" });

        Assert.Throws<TenantNotResolvedException>(() => context.SaveChanges());
    }

    [Fact]
    public void SaveChanges_throws_when_tenant_mismatch()
    {
        var currentTenant = new CurrentTenantAccessor
        {
            Current = CreateTenantContext("tenant-1")
        };
        var options = new EfCoreMultitenancyOptions
        {
            Mode = TenantIsolationMode.SharedDatabase,
            EnableSaveChangesEnforcement = true,
            UseShadowTenantId = false
        };

        using var context = CreateContext(currentTenant, options, useInterceptor: true);

        var record = new TenantRecord { Id = 1, TenantId = "tenant-2", Name = "Existing" };
        context.Attach(record);
        context.Entry(record).State = EntityState.Modified;

        Assert.Throws<InvalidOperationException>(() => context.SaveChanges());
    }

    private static TestTenantDbContext CreateContext(
        CurrentTenantAccessor currentTenant,
        EfCoreMultitenancyOptions options,
        bool useInterceptor = false)
    {
        var builder = new DbContextOptionsBuilder<TestTenantDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        if (useInterceptor)
        {
            var interceptor = new TenantSaveChangesInterceptor(
                currentTenant,
                OptionsFactory.Create(options));
            builder.AddInterceptors(interceptor);
        }

        return new TestTenantDbContext(
            builder.Options,
            currentTenant,
            OptionsFactory.Create(options),
            new TenantModelCustomizer());
    }

    private static TenantContext CreateTenantContext(string tenantId)
    {
        var tenantInfo = new TenantInfo(tenantId) { IsActive = true, State = TenantState.Active };
        var resolution = TenantResolutionResult.Resolved(tenantId, TenantResolutionSource.Header);
        return new TenantContext(tenantInfo, resolution);
    }

    private sealed class TestTenantDbContext : TenantDbContext
    {
        public TestTenantDbContext(
            DbContextOptions<TestTenantDbContext> options,
            ICurrentTenant currentTenant,
            IOptions<EfCoreMultitenancyOptions> optionsAccessor,
            ITenantModelCustomizer modelCustomizer)
            : base(options, currentTenant, optionsAccessor, modelCustomizer)
        {
        }

        public DbSet<TenantRecord> Records => Set<TenantRecord>();
    }

    private sealed class TenantRecord : ITenantEntity
    {
        public int Id { get; set; }

        public string TenantId { get; set; } = string.Empty;

        public string? Name { get; set; }
    }
}
