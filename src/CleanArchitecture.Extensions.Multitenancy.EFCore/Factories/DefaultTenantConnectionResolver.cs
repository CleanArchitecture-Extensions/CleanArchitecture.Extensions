using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Factories;

/// <summary>
/// Resolves tenant-specific connection strings from configuration.
/// </summary>
public sealed class DefaultTenantConnectionResolver : ITenantConnectionResolver
{
    private readonly EfCoreMultitenancyOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultTenantConnectionResolver"/> class.
    /// </summary>
    /// <param name="options">EF Core multitenancy options.</param>
    public DefaultTenantConnectionResolver(IOptions<EfCoreMultitenancyOptions> options)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public string? ResolveConnectionString(ITenantInfo? tenant)
        => _options.ResolveConnectionString(tenant);
}
