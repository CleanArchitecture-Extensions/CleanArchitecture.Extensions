using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Migrations;

/// <summary>
/// Runs EF Core migrations for each tenant.
/// </summary>
/// <typeparam name="TContext">DbContext type.</typeparam>
public sealed class TenantMigrationRunner<TContext>
    where TContext : DbContext
{
    private readonly ITenantAccessor _tenantAccessor;
    private readonly ITenantDbContextFactory<TContext> _dbContextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantMigrationRunner{TContext}"/> class.
    /// </summary>
    /// <param name="tenantAccessor">Tenant accessor.</param>
    /// <param name="dbContextFactory">Tenant-aware DbContext factory.</param>
    public TenantMigrationRunner(
        ITenantAccessor tenantAccessor,
        ITenantDbContextFactory<TContext> dbContextFactory)
    {
        _tenantAccessor = tenantAccessor ?? throw new ArgumentNullException(nameof(tenantAccessor));
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    /// <summary>
    /// Runs migrations for each tenant in order.
    /// </summary>
    /// <param name="tenants">Tenant metadata list.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task RunAsync(IEnumerable<ITenantInfo> tenants, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenants);

        foreach (var tenant in tenants)
        {
            if (tenant is null)
            {
                continue;
            }

            var resolution = TenantResolutionResult.Resolved(tenant.TenantId, TenantResolutionSource.Default);
            var context = new TenantContext(tenant, resolution, isValidated: true);

            using var scope = _tenantAccessor.BeginScope(context);
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
