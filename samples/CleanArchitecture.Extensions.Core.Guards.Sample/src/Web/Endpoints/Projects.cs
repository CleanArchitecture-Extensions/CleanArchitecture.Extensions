using CleanArchitecture.Extensions.Core.Guards.Sample.Application.Projects.Commands.ArchiveProjectWithThrow;
using CleanArchitecture.Extensions.Core.Guards.Sample.Application.Projects.Commands.CreateProject;
using CleanArchitecture.Extensions.Core.Guards.Sample.Application.Projects.Commands.ImportProjects;
using CoreResults = CleanArchitecture.Extensions.Core.Results;

namespace CleanArchitecture.Extensions.Core.Guards.Sample.Web.Endpoints;

public class Projects : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateProject);
        groupBuilder.MapPost(ImportProjects, "import");
        groupBuilder.MapPost(ArchiveProject, "{id:int}/archive");
    }

    public async Task<IResult> CreateProject(ISender sender, CreateProjectCommand command)
    {
        var result = await sender.Send(command);

        return result.Match<IResult>(
            id => TypedResults.Created($"/api/{nameof(Projects)}/{id}", new { id, traceId = result.TraceId }),
            _ => ToProblemResult("Project validation failed.", result));
    }

    public async Task<IResult> ImportProjects(ISender sender, ImportProjectsCommand command)
    {
        var result = await sender.Send(command);

        return result.Match<IResult>(
            ids => TypedResults.Ok(new { ids, traceId = result.TraceId }),
            _ => ToProblemResult("Import validation failed.", result));
    }

    public async Task<IResult> ArchiveProject(ISender sender, int id)
    {
        try
        {
            await sender.Send(new ArchiveProjectWithThrowCommand(id));
            return TypedResults.NoContent();
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(
                title: "Archiving failed",
                statusCode: StatusCodes.Status400BadRequest,
                detail: ex.Message);
        }
    }

    private static IResult ToProblemResult<T>(string title, CoreResults.Result<T> result, int statusCode = StatusCodes.Status400BadRequest)
    {
        var errors = result.Errors.Select(e => new { e.Code, e.Message, e.Metadata });

        return TypedResults.Problem(title: title,
            statusCode: statusCode,
            extensions: new Dictionary<string, object?>
            {
                ["errors"] = errors,
                ["traceId"] = result.TraceId
            });
    }
}
