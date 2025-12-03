using System.Globalization;
using CleanArchitecture.Extensions.Core.Guards.Sample.Application.Common.Interfaces;
using CleanArchitecture.Extensions.Core.Guards.Sample.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using CoreResults = CleanArchitecture.Extensions.Core.Results;
using CoreGuard = CleanArchitecture.Extensions.Core.Guards.Guard;
using CoreGuardOptions = CleanArchitecture.Extensions.Core.Guards.GuardOptions;
using GuardStrategy = CleanArchitecture.Extensions.Core.Guards.GuardStrategy;

namespace CleanArchitecture.Extensions.Core.Guards.Sample.Application.Projects.Commands.ImportProjects;

public sealed record ImportProjectsCommand(IEnumerable<ImportProjectRequest> Projects) : IRequest<CoreResults.Result<IReadOnlyList<int>>>;

public sealed record ImportProjectRequest(string Name, string? Description, decimal Budget);

public sealed class ImportProjectsCommandHandler : IRequestHandler<ImportProjectsCommand, CoreResults.Result<IReadOnlyList<int>>>
{
    private const int MaxNameLength = 120;
    private readonly IApplicationDbContext _context;

    public ImportProjectsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CoreResults.Result<IReadOnlyList<int>>> Handle(ImportProjectsCommand request, CancellationToken cancellationToken)
    {
        var traceId = Guid.NewGuid().ToString("N");
        var sink = new List<CleanArchitecture.Extensions.Core.Results.Error>();
        var guardOptions = new CoreGuardOptions
        {
            Strategy = GuardStrategy.Accumulate,
            ErrorSink = sink,
            TraceId = traceId
        };

        var projects = new List<Project>();
        var incoming = request.Projects.ToList();

        for (var index = 0; index < incoming.Count; index++)
        {
            var project = incoming[index];
            var beforeCount = sink.Count;
            var nameResult = CoreGuard.AgainstNullOrWhiteSpace(project.Name, nameof(project.Name), guardOptions)
                .Ensure(n => n.Length <= MaxNameLength, new CoreResults.Error("projects.name.length", $"Name must be {MaxNameLength} characters or fewer.", traceId));

            var budgetResult = CoreGuard.Ensure(project.Budget >= 0, "projects.budget.range", "Budget cannot be negative.", guardOptions);

            if (nameResult.IsSuccess && budgetResult.IsSuccess)
            {
                projects.Add(new Project(project.Name.Trim(), project.Description, project.Budget));
            }
            else
            {
                AddRowMetadata(sink, beforeCount, index + 1);
            }
        }

        if (sink.Count > 0)
        {
            return CoreResults.Result.Failure<IReadOnlyList<int>>(sink, traceId);
        }

        _context.Projects.AddRange(projects);
        await _context.SaveChangesAsync(cancellationToken);

        return CoreResults.Result.Success<IReadOnlyList<int>>(projects.Select(p => p.Id).ToList(), traceId);
    }

    private static void AddRowMetadata(ICollection<CleanArchitecture.Extensions.Core.Results.Error> errors, int previousErrorCount, int rowNumber)
    {
        var withRow = errors.Skip(previousErrorCount).ToList();

        foreach (var error in withRow)
        {
            errors.Remove(error);
            errors.Add(error.WithMetadata("row", rowNumber.ToString(CultureInfo.InvariantCulture)));
        }
    }
}
