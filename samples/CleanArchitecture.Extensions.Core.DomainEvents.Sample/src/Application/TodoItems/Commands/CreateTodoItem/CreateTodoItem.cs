using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Application.Common.Interfaces;
using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Domain.Entities;
using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Domain.Events;
using CleanArchitecture.Extensions.Core.Logging;

namespace CleanArchitecture.Extensions.Core.DomainEvents.Sample.Application.TodoItems.Commands.CreateTodoItem;

public record CreateTodoItemCommand : IRequest<int>
{
    public int ListId { get; init; }

    public string? Title { get; init; }
}

public class CreateTodoItemCommandHandler : IRequestHandler<CreateTodoItemCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogContext _logContext;

    public CreateTodoItemCommandHandler(IApplicationDbContext context, ILogContext logContext)
    {
        _context = context;
        _logContext = logContext;
    }

    public async Task<int> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
    {
        var correlationId = _logContext.CorrelationId ?? Guid.NewGuid().ToString("N");
        _logContext.CorrelationId = correlationId;

        var entity = new TodoItem
        {
            ListId = request.ListId,
            Title = request.Title,
            Done = false
        };

        entity.AddDomainEvent(new TodoItemCreatedEvent(entity, correlationId));

        _context.TodoItems.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
