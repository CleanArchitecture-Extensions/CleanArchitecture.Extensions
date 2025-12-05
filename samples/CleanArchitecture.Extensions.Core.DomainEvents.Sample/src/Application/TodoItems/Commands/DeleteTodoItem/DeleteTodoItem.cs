using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Application.Common.Interfaces;
using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Domain.Events;
using CleanArchitecture.Extensions.Core.Logging;

namespace CleanArchitecture.Extensions.Core.DomainEvents.Sample.Application.TodoItems.Commands.DeleteTodoItem;

public record DeleteTodoItemCommand(int Id) : IRequest;

public class DeleteTodoItemCommandHandler : IRequestHandler<DeleteTodoItemCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogContext _logContext;

    public DeleteTodoItemCommandHandler(IApplicationDbContext context, ILogContext logContext)
    {
        _context = context;
        _logContext = logContext;
    }

    public async Task Handle(DeleteTodoItemCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TodoItems
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        var correlationId = _logContext.CorrelationId ?? Guid.NewGuid().ToString("N");
        _logContext.CorrelationId = correlationId;

        _context.TodoItems.Remove(entity);

        entity.AddDomainEvent(new TodoItemDeletedEvent(entity, correlationId));

        await _context.SaveChangesAsync(cancellationToken);
    }
}
