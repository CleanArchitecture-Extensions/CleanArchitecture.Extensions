using CleanArchitecture.Extensions.Multitenancy.EFCore;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;

/// <summary>
/// Applies tenant-specific model configuration to EF Core.
/// </summary>
public interface ITenantModelCustomizer
{
    /// <summary>
    /// Applies tenant-specific model configuration.
    /// </summary>
    /// <param name="modelBuilder">Model builder.</param>
    /// <param name="context">DbContext instance.</param>
    /// <param name="options">EF Core multitenancy options.</param>
    void Customize(ModelBuilder modelBuilder, TenantDbContext context, EfCoreMultitenancyOptions options);
}
