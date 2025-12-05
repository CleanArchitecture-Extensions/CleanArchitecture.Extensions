using CleanArchitecture.Extensions.Core.Time.Sample.Domain.Entities;

namespace CleanArchitecture.Extensions.Core.Time.Sample.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
