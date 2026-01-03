using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore;

/// <summary>
/// EF Core configuration helpers for multitenancy.
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Replaces the model cache key factory with a tenant-aware implementation.
    /// </summary>
    /// <param name="optionsBuilder">DbContext options builder.</param>
    /// <param name="serviceProvider">Application service provider.</param>
    public static DbContextOptionsBuilder UseTenantModelCacheKeyFactory(
        this DbContextOptionsBuilder optionsBuilder,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var options = serviceProvider.GetRequiredService<IOptions<EfCoreMultitenancyOptions>>().Value;
        return optionsBuilder.UseTenantModelCacheKeyFactory(options);
    }

    /// <summary>
    /// Replaces the model cache key factory with a tenant-aware implementation.
    /// </summary>
    /// <param name="optionsBuilder">DbContext options builder.</param>
    /// <param name="options">Resolved multitenancy options.</param>
    public static DbContextOptionsBuilder UseTenantModelCacheKeyFactory(
        this DbContextOptionsBuilder optionsBuilder,
        EfCoreMultitenancyOptions options)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);
        ArgumentNullException.ThrowIfNull(options);

        var infrastructure = (IDbContextOptionsBuilderInfrastructure)optionsBuilder;
        infrastructure.AddOrUpdateExtension(new TenantModelCacheKeyOptionsExtension(options));

        return optionsBuilder.ReplaceService<IModelCacheKeyFactory, TenantModelCacheKeyFactory>();
    }
}
