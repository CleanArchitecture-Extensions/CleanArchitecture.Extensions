using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Factories;

/// <summary>
/// Default tenant-aware DbContext factory that delegates to EF Core's factory.
/// </summary>
/// <typeparam name="TContext">DbContext type.</typeparam>
public sealed class TenantDbContextFactory<TContext> : ITenantDbContextFactory<TContext>
    where TContext : DbContext
{
    private readonly IDbContextFactory<TContext> _innerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantDbContextFactory{TContext}"/> class.
    /// </summary>
    /// <param name="innerFactory">EF Core DbContext factory.</param>
    public TenantDbContextFactory(IDbContextFactory<TContext> innerFactory)
    {
        _innerFactory = innerFactory ?? throw new ArgumentNullException(nameof(innerFactory));
    }

    /// <inheritdoc />
    public TContext CreateDbContext() => _innerFactory.CreateDbContext();

    /// <inheritdoc />
    public Task<TContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => _innerFactory.CreateDbContextAsync(cancellationToken);
}
