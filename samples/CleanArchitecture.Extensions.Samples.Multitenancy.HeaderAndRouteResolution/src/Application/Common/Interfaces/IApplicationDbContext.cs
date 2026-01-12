using CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Domain.Entities;

namespace CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
