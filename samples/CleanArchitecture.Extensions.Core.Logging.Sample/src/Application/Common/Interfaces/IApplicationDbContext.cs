using CleanArchitecture.Extensions.Core.Logging.Sample.Domain.Entities;

namespace CleanArchitecture.Extensions.Core.Logging.Sample.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
