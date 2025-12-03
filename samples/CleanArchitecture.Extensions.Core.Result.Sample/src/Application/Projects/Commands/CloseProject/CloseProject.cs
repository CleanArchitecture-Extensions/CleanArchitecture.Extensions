using CleanArchitecture.Extensions.Core.Result.Sample.Application.Common.Interfaces;
using CleanArchitecture.Extensions.Core.Result.Sample.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using CoreResults = CleanArchitecture.Extensions.Core.Results;
using Guard = CleanArchitecture.Extensions.Core.Guards.Guard;
using GuardOptions = CleanArchitecture.Extensions.Core.Guards.GuardOptions;

namespace CleanArchitecture.Extensions.Core.Result.Sample.Application.Projects.Commands.CloseProject;

public sealed record CloseProjectCommand(int Id) : IRequest<CoreResults.Result>;

public sealed class CloseProjectCommandHandler : IRequestHandler<CloseProjectCommand, CoreResults.Result>
{
    private readonly IApplicationDbContext _context;

    public CloseProjectCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CoreResults.Result> Handle(CloseProjectCommand request, CancellationToken cancellationToken)
    {
        var traceId = Guid.NewGuid().ToString("N");
        var guardOptions = new GuardOptions { TraceId = traceId };

        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        var closeResult = Guard.AgainstNull(project, nameof(project), guardOptions)
            .Bind(p => EnsureNotClosed(p, traceId))
            .Tap(p => p.Close(DateTimeOffset.UtcNow));

        if (closeResult.IsFailure)
        {
            return CoreResults.Result.Failure(closeResult.Errors, traceId);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return CoreResults.Result.Success(traceId);
    }

    private static CoreResults.Result<Project> EnsureNotClosed(Project project, string traceId)
    {
        return project.IsClosed
            ? CoreResults.Result.Failure<Project>(new CoreResults.Error("projects.closed", "Project is already closed.", traceId), traceId)
            : CoreResults.Result.Success(project, traceId);
    }
}
