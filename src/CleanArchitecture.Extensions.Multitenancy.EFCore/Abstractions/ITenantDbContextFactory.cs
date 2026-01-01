using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;

/// <summary>
/// Creates tenant-aware DbContext instances.
/// </summary>
/// <typeparam name="TContext">DbContext type.</typeparam>
public interface ITenantDbContextFactory<TContext>
    where TContext : DbContext
{
    /// <summary>
    /// Creates a DbContext for the current tenant scope.
    /// </summary>
    TContext CreateDbContext();

    /// <summary>
    /// Creates a DbContext for the current tenant scope asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<TContext> CreateDbContextAsync(CancellationToken cancellationToken = default);
}
