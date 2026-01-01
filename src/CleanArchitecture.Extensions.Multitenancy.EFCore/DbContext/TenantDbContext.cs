using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore;

/// <summary>
/// Base DbContext that applies tenant-aware model configuration.
/// </summary>
public abstract class TenantDbContext : Microsoft.EntityFrameworkCore.DbContext, ITenantDbContext
{
    private readonly ICurrentTenant _currentTenant;
    private readonly EfCoreMultitenancyOptions _options;
    private readonly ITenantModelCustomizer _modelCustomizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantDbContext"/> class.
    /// </summary>
    /// <param name="options">DbContext options.</param>
    /// <param name="currentTenant">Current tenant accessor.</param>
    /// <param name="optionsAccessor">EF Core multitenancy options.</param>
    /// <param name="modelCustomizer">Tenant model customizer.</param>
    protected TenantDbContext(
        DbContextOptions options,
        ICurrentTenant currentTenant,
        IOptions<EfCoreMultitenancyOptions> optionsAccessor,
        ITenantModelCustomizer modelCustomizer)
        : base(options)
    {
        _currentTenant = currentTenant ?? throw new ArgumentNullException(nameof(currentTenant));
        _options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
        _modelCustomizer = modelCustomizer ?? throw new ArgumentNullException(nameof(modelCustomizer));
    }

    /// <summary>
    /// Gets the current tenant identifier.
    /// </summary>
    public string? CurrentTenantId => _currentTenant.TenantId;

    /// <summary>
    /// Gets the current tenant metadata.
    /// </summary>
    public ITenantInfo? CurrentTenantInfo => _currentTenant.TenantInfo;

    /// <summary>
    /// Gets the schema name for the current tenant.
    /// </summary>
    public string? CurrentSchema => _options.ResolveSchemaName(CurrentTenantInfo);

    /// <summary>
    /// Applies tenant-specific model configuration.
    /// </summary>
    /// <param name="modelBuilder">Model builder.</param>
    protected void ApplyTenantModel(ModelBuilder modelBuilder)
    {
        _modelCustomizer.Customize(modelBuilder, this, _options);
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ApplyTenantModel(modelBuilder);
    }
}
