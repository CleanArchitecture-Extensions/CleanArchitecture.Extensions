using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore;

/// <summary>
/// Adds tenant-specific schema information to the EF Core model cache key.
/// </summary>
public sealed class TenantModelCacheKeyFactory : IModelCacheKeyFactory
{
    private readonly EfCoreMultitenancyOptions _options;
    private readonly ModelCacheKeyFactory _innerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantModelCacheKeyFactory"/> class.
    /// </summary>
    /// <param name="options">EF Core multitenancy options.</param>
    /// <param name="dependencies">Model cache key factory dependencies.</param>
    public TenantModelCacheKeyFactory(
        IOptions<EfCoreMultitenancyOptions> options,
        ModelCacheKeyFactoryDependencies dependencies)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _innerFactory = new ModelCacheKeyFactory(dependencies ?? throw new ArgumentNullException(nameof(dependencies)));
    }

    /// <inheritdoc />
    public object Create(DbContext context, bool designTime)
    {
        ArgumentNullException.ThrowIfNull(context);

        var baseKey = _innerFactory.Create(context, designTime);
        if (!_options.IncludeSchemaInModelCacheKey || _options.Mode != TenantIsolationMode.SchemaPerTenant)
        {
            return baseKey;
        }

        if (context is ITenantDbContext tenantContext)
        {
            var schema = _options.ResolveSchemaName(tenantContext.CurrentTenantInfo) ?? string.Empty;
            return new TenantModelCacheKey(baseKey, schema);
        }

        return baseKey;
    }

    private sealed record TenantModelCacheKey(object BaseKey, string Schema);
}
