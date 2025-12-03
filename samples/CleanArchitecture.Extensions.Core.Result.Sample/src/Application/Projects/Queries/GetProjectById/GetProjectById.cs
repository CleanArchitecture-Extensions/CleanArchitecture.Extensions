using CleanArchitecture.Extensions.Core.Result.Sample.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using CoreResults = CleanArchitecture.Extensions.Core.Results;
using Guard = CleanArchitecture.Extensions.Core.Guards.Guard;
using GuardOptions = CleanArchitecture.Extensions.Core.Guards.GuardOptions;

namespace CleanArchitecture.Extensions.Core.Result.Sample.Application.Projects.Queries.GetProjectById;

public sealed record GetProjectByIdQuery(int Id) : IRequest<CoreResults.Result<ProjectSummaryDto>>;

public sealed class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, CoreResults.Result<ProjectSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProjectByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CoreResults.Result<ProjectSummaryDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var traceId = Guid.NewGuid().ToString("N");
        var guardOptions = new GuardOptions { TraceId = traceId };

        var project = await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        var projectResult = Guard.AgainstNull(project, nameof(project), guardOptions);

        return projectResult.Map(p => new ProjectSummaryDto
        {
            Id = p.Id,
            Name = p.Name,
            Budget = p.Budget,
            IsClosed = p.IsClosed,
            ClosedOn = p.ClosedOn
        }, traceId);
    }
}

public sealed class ProjectSummaryDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public decimal Budget { get; init; }

    public bool IsClosed { get; init; }

    public DateTimeOffset? ClosedOn { get; init; }
}
