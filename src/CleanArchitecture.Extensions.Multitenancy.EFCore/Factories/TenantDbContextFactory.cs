using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;
using CleanArchitecture.Extensions.Multitenancy.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Factories;

/// <summary>
/// Default tenant-aware DbContext factory that delegates to EF Core's factory.
/// </summary>
/// <typeparam name="TContext">DbContext type.</typeparam>
public sealed class TenantDbContextFactory<TContext> : ITenantDbContextFactory<TContext>
    where TContext : DbContext
{
    private readonly IDbContextFactory<TContext> _innerFactory;
    private readonly ICurrentTenant? _currentTenant;
    private readonly ITenantConnectionResolver? _connectionResolver;
    private readonly EfCoreMultitenancyOptions? _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantDbContextFactory{TContext}"/> class.
    /// </summary>
    /// <param name="innerFactory">EF Core DbContext factory.</param>
    public TenantDbContextFactory(IDbContextFactory<TContext> innerFactory)
    {
        _innerFactory = innerFactory ?? throw new ArgumentNullException(nameof(innerFactory));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantDbContextFactory{TContext}"/> class with connection resolution.
    /// </summary>
    /// <param name="innerFactory">EF Core DbContext factory.</param>
    /// <param name="currentTenant">Current tenant accessor.</param>
    /// <param name="connectionResolver">Tenant connection resolver.</param>
    /// <param name="options">EF Core multitenancy options.</param>
    public TenantDbContextFactory(
        IDbContextFactory<TContext> innerFactory,
        ICurrentTenant currentTenant,
        ITenantConnectionResolver connectionResolver,
        IOptions<EfCoreMultitenancyOptions> options)
        : this(innerFactory)
    {
        _currentTenant = currentTenant ?? throw new ArgumentNullException(nameof(currentTenant));
        _connectionResolver = connectionResolver ?? throw new ArgumentNullException(nameof(connectionResolver));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public TContext CreateDbContext() => ConfigureContext(_innerFactory.CreateDbContext());

    /// <inheritdoc />
    public Task<TContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => CreateDbContextAsyncInternal(cancellationToken);

    private async Task<TContext> CreateDbContextAsyncInternal(CancellationToken cancellationToken)
    {
        var context = await _innerFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        return ConfigureContext(context);
    }

    private TContext ConfigureContext(TContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (_options is null || _currentTenant is null || _connectionResolver is null)
        {
            return context;
        }

        if (_options.Mode != TenantIsolationMode.DatabasePerTenant)
        {
            return context;
        }

        if (string.IsNullOrWhiteSpace(_currentTenant.TenantId))
        {
            throw new TenantNotResolvedException("Tenant context is required for database-per-tenant mode.");
        }

        var connectionString = _connectionResolver.ResolveConnectionString(_currentTenant.TenantInfo);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"No connection string resolved for tenant '{_currentTenant.TenantId}'. Configure EfCoreMultitenancyOptions.ConnectionStringFormat/ConnectionStringProvider or register ITenantConnectionResolver.");
        }

        if (!context.Database.IsRelational())
        {
            throw new InvalidOperationException("Database-per-tenant mode requires a relational EF Core provider.");
        }

        context.Database.SetConnectionString(connectionString);
        return context;
    }
}
