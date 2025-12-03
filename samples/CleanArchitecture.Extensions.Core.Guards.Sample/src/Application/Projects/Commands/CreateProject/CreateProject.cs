using CleanArchitecture.Extensions.Core.Guards.Sample.Application.Common.Interfaces;
using CleanArchitecture.Extensions.Core.Guards.Sample.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using CoreResults = CleanArchitecture.Extensions.Core.Results;
using CoreGuard = CleanArchitecture.Extensions.Core.Guards.Guard;
using CoreGuardOptions = CleanArchitecture.Extensions.Core.Guards.GuardOptions;

namespace CleanArchitecture.Extensions.Core.Guards.Sample.Application.Projects.Commands.CreateProject;

public sealed record CreateProjectCommand(string Name, string? Description, decimal Budget) : IRequest<CoreResults.Result<int>>;

public sealed class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, CoreResults.Result<int>>
{
    private const int MaxNameLength = 120;
    private const int MaxDescriptionLength = 1024;

    private readonly IApplicationDbContext _context;

    public CreateProjectCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CoreResults.Result<int>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var traceId = Guid.NewGuid().ToString("N");
        var guardOptions = new CoreGuardOptions { TraceId = traceId };

        var name = CoreGuard.AgainstNullOrWhiteSpace(request.Name, nameof(request.Name), guardOptions)
            .Ensure(n => n.Length <= MaxNameLength, new CoreResults.Error("projects.name.length", $"Name must be {MaxNameLength} characters or fewer.", traceId));

        var description = string.IsNullOrWhiteSpace(request.Description)
            ? CoreResults.Result.Success<string?>(null, traceId)
            : CoreGuard.AgainstTooLong(request.Description, MaxDescriptionLength, nameof(request.Description), guardOptions)
                .Map(value => (string?)value.Trim(), traceId);

        var budget = CoreGuard.Ensure(request.Budget >= 0,
            "projects.budget.range",
            "Budget cannot be negative.",
            guardOptions);

        var validation = CoreResults.Result.Combine(name, description, budget);
        if (validation.IsFailure)
        {
            return CoreResults.Result.Failure<int>(validation.Errors, traceId);
        }

        var duplicate = await _context.Projects.AnyAsync(p => p.Name == name.Value, cancellationToken);
        if (duplicate)
        {
            var duplicateError = new CoreResults.Error("projects.name.duplicate", "Project name already exists.", traceId)
                .WithMetadata("name", name.Value);
            return CoreResults.Result.Failure<int>(duplicateError, traceId);
        }

        var project = new Project(name.Value, description.ValueOrDefault, request.Budget);
        _context.Projects.Add(project);

        await _context.SaveChangesAsync(cancellationToken);

        return CoreResults.Result.Success(project.Id, traceId);
    }
}
