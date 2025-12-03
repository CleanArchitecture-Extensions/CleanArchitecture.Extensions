using CleanArchitecture.Extensions.Core.Result.Sample.Domain.Entities;

namespace CleanArchitecture.Extensions.Core.Result.Sample.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    DbSet<Project> Projects { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
