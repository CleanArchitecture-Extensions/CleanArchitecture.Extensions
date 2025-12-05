using CleanArchitecture.Extensions.Core.Options.Sample.Domain.Entities;

namespace CleanArchitecture.Extensions.Core.Options.Sample.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
