using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Context;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Filters;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Interceptors;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Tests;

internal sealed class TenantWidget : ITenantEntity
{
    public int Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string? Name { get; set; }
}

[GlobalEntity]
internal sealed class GlobalWidget
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

internal sealed class ShadowWidget
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

internal sealed class TestDbContext : TenantDbContext
{
    public TestDbContext(
        DbContextOptions<TestDbContext> options,
        ICurrentTenant currentTenant,
        IOptions<EfCoreMultitenancyOptions> optionsAccessor,
        ITenantModelCustomizer modelCustomizer)
        : base(options, currentTenant, optionsAccessor, modelCustomizer)
    {
    }

    public DbSet<TenantWidget> TenantWidgets => Set<TenantWidget>();
    public DbSet<GlobalWidget> GlobalWidgets => Set<GlobalWidget>();
    public DbSet<ShadowWidget> ShadowWidgets => Set<ShadowWidget>();
}

internal static class TestTenant
{
    public static TenantContext Create(string tenantId)
    {
        var tenant = new TenantInfo(tenantId)
        {
            Name = $"Tenant {tenantId}",
            IsActive = true
        };

        var resolution = TenantResolutionResult.Resolved(tenantId, TenantResolutionSource.Default);
        return new TenantContext(tenant, resolution, isValidated: true);
    }
}

internal static class TestDbContextFactory
{
    public static TestDbContext Create(
        CurrentTenantAccessor accessor,
        EfCoreMultitenancyOptions options,
        bool addInterceptor = false,
        string? databaseName = null)
    {
        var builder = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString());

        if (addInterceptor)
        {
            builder.AddInterceptors(new TenantSaveChangesInterceptor(accessor, Microsoft.Extensions.Options.Options.Create(options)));
        }

        return new TestDbContext(
            builder.Options,
            accessor,
            Microsoft.Extensions.Options.Options.Create(options),
            new TenantModelCustomizer());
    }
}
