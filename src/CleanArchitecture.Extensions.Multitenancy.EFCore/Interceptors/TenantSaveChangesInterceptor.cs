using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Filters;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;
using CleanArchitecture.Extensions.Multitenancy.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Interceptors;

/// <summary>
/// Ensures tenant identifiers are enforced during SaveChanges.
/// </summary>
public sealed class TenantSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentTenant _currentTenant;
    private readonly EfCoreMultitenancyOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantSaveChangesInterceptor"/> class.
    /// </summary>
    /// <param name="currentTenant">Current tenant accessor.</param>
    /// <param name="options">EF Core multitenancy options.</param>
    public TenantSaveChangesInterceptor(ICurrentTenant currentTenant, IOptions<EfCoreMultitenancyOptions> options)
    {
        _currentTenant = currentTenant ?? throw new ArgumentNullException(nameof(currentTenant));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        EnforceTenant(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        EnforceTenant(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void EnforceTenant(DbContext? context)
    {
        if (context is null || !_options.EnableSaveChangesEnforcement)
        {
            return;
        }

        var tenantId = _currentTenant.TenantId;
        var hasTenantChanges = false;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
            {
                continue;
            }

            if (!TenantEntityMetadata.IsTenantScoped(entry.Metadata, _options))
            {
                continue;
            }

            hasTenantChanges = true;
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                continue;
            }

            var property = entry.Metadata.FindProperty(_options.TenantIdPropertyName);
            if (property is null)
            {
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                entry.Property(_options.TenantIdPropertyName).CurrentValue = tenantId;
                continue;
            }

            var currentValue = entry.Property(_options.TenantIdPropertyName).CurrentValue?.ToString();
            if (!string.Equals(currentValue, tenantId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Tenant mismatch for '{entry.Metadata.ClrType.Name}'. Current tenant '{tenantId}' does not match entity tenant '{currentValue}'.");
            }
        }

        if (string.IsNullOrWhiteSpace(tenantId) && hasTenantChanges && _options.RequireTenantForWrites)
        {
            throw new TenantNotResolvedException("Tenant context is required for data modifications.");
        }
    }
}
