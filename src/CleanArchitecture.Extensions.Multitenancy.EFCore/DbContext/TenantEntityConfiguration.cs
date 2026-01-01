using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore;

/// <summary>
/// Base configuration helper for tenant-scoped entities.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
public abstract class TenantEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : class
{
    private readonly string _tenantIdPropertyName;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantEntityConfiguration{TEntity}"/> class.
    /// </summary>
    /// <param name="tenantIdPropertyName">Tenant identifier property name.</param>
    protected TenantEntityConfiguration(string? tenantIdPropertyName = null)
    {
        _tenantIdPropertyName = string.IsNullOrWhiteSpace(tenantIdPropertyName) ? "TenantId" : tenantIdPropertyName;
    }

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        ConfigureEntity(builder);
        ConfigureTenant(builder);
    }

    /// <summary>
    /// Configures entity-specific mappings.
    /// </summary>
    /// <param name="builder">Entity type builder.</param>
    protected virtual void ConfigureEntity(EntityTypeBuilder<TEntity> builder)
    {
    }

    /// <summary>
    /// Configures tenant identifier mapping for the entity.
    /// </summary>
    /// <param name="builder">Entity type builder.</param>
    protected virtual void ConfigureTenant(EntityTypeBuilder<TEntity> builder)
    {
        if (typeof(IGlobalEntity).IsAssignableFrom(typeof(TEntity)))
        {
            return;
        }

        if (typeof(ITenantEntity).IsAssignableFrom(typeof(TEntity)))
        {
            builder.Property(_tenantIdPropertyName).IsRequired();
            return;
        }

        builder.Property<string>(_tenantIdPropertyName);
    }
}
