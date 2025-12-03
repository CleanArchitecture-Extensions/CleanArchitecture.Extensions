using CleanArchitecture.Extensions.Core.Result.Sample.Application.Common.Interfaces;
using CleanArchitecture.Extensions.Core.Result.Sample.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using CoreResults = CleanArchitecture.Extensions.Core.Results;
using GuardOptions = CleanArchitecture.Extensions.Core.Guards.GuardOptions;
using Guard = CleanArchitecture.Extensions.Core.Guards.Guard;

namespace CleanArchitecture.Extensions.Core.Result.Sample.Application.Projects.Commands.CreateProject;

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
        var guardOptions = new GuardOptions { TraceId = traceId };

        var name = Guard.AgainstNullOrWhiteSpace(request.Name, nameof(request.Name), guardOptions)
            .Ensure(n => n.Length <= MaxNameLength, new CoreResults.Error("projects.name.length", $"Project name must be {MaxNameLength} characters or fewer.", traceId));

        var description = Guard.Ensure(request.Description is null || request.Description.Length <= MaxDescriptionLength,
            "projects.description.length",
            $"Description must be {MaxDescriptionLength} characters or fewer.",
            guardOptions);

        var budget = Guard.Ensure(request.Budget >= 0,
            "projects.budget.range",
            "Budget cannot be negative.",
            guardOptions);

        var validation = CoreResults.Result.Combine(name, description, budget);
        if (validation.IsFailure)
        {
            return CoreResults.Result.Failure<int>(validation.Errors, traceId);
        }

        var project = new Project(name.Value, request.Description, request.Budget);

        var duplicateName = await _context.Projects
            .AnyAsync(p => p.Name == project.Name, cancellationToken);

        if (duplicateName)
        {
            var duplicateError = new CoreResults.Error("projects.name.duplicate", "A project with this name already exists.", traceId)
                .WithMetadata("name", project.Name);

            return CoreResults.Result.Failure<int>(duplicateError, traceId);
        }

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        return CoreResults.Result.Success(project.Id, traceId);
    }
}
