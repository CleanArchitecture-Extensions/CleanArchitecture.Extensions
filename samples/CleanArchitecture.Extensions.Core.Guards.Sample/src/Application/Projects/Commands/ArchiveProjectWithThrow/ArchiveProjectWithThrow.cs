using CleanArchitecture.Extensions.Core.Guards.Sample.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using CoreGuard = CleanArchitecture.Extensions.Core.Guards.Guard;
using CoreGuardOptions = CleanArchitecture.Extensions.Core.Guards.GuardOptions;
using GuardStrategy = CleanArchitecture.Extensions.Core.Guards.GuardStrategy;

namespace CleanArchitecture.Extensions.Core.Guards.Sample.Application.Projects.Commands.ArchiveProjectWithThrow;

public sealed record ArchiveProjectWithThrowCommand(int Id) : IRequest;

public sealed class ArchiveProjectWithThrowCommandHandler : IRequestHandler<ArchiveProjectWithThrowCommand>
{
    private readonly IApplicationDbContext _context;

    public ArchiveProjectWithThrowCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ArchiveProjectWithThrowCommand request, CancellationToken cancellationToken)
    {
        var guardOptions = new CoreGuardOptions
        {
            Strategy = GuardStrategy.Throw,
            ExceptionFactory = error => new InvalidOperationException($"{error.Code}: {error.Message}")
        };

        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        CoreGuard.AgainstNull(project, nameof(project), guardOptions);
        CoreGuard.Ensure(!project!.IsArchived, "projects.archived", "Project is already archived.", guardOptions);

        project.Archive(DateTimeOffset.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
