using CleanArchitecture.Extensions.Core.Guards.Sample.Domain.Entities;

namespace CleanArchitecture.Extensions.Core.Guards.Sample.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    DbSet<Project> Projects { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
