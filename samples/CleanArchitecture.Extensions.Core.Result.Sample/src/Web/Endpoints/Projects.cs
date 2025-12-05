using CleanArchitecture.Extensions.Core.Result.Sample.Application.Projects.Commands.CloseProject;
using CleanArchitecture.Extensions.Core.Result.Sample.Application.Projects.Commands.CreateProject;
using CleanArchitecture.Extensions.Core.Result.Sample.Application.Projects.Queries.GetProjectById;
using Microsoft.AspNetCore.Http.HttpResults;
using CoreResults = CleanArchitecture.Extensions.Core.Results;

namespace CleanArchitecture.Extensions.Core.Result.Sample.Web.Endpoints;

public class Projects : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateProject);
        groupBuilder.MapGet(GetProjectById, "{id:int}");
        groupBuilder.MapPost(CloseProject, "{id:int}/close");
    }

    public async Task<Results<Created<object>, ProblemHttpResult>> CreateProject(ISender sender, CreateProjectCommand command)
    {
        var result = await sender.Send(command);

        return result.Match<Results<Created<object>, ProblemHttpResult>>(
            id => TypedResults.Created<object>($"/api/{nameof(Projects)}/{id}", new { id, traceId = result.TraceId }),
            _ => ToProblemResult("Project validation failed.", result));
    }

    public async Task<Results<Ok<object>, ProblemHttpResult>> GetProjectById(ISender sender, int id)
    {
        var result = await sender.Send(new GetProjectByIdQuery(id));

        return result.Match<Results<Ok<object>, ProblemHttpResult>>(
            project => TypedResults.Ok<object>(new { project, traceId = result.TraceId }),
            _ => ToProblemResult("Project not found.", result, StatusCodes.Status404NotFound));
    }

    public async Task<Results<NoContent, ProblemHttpResult>> CloseProject(ISender sender, int id)
    {
        var result = await sender.Send(new CloseProjectCommand(id));

        if (result.IsSuccess)
        {
            return TypedResults.NoContent();
        }

        var statusCode = result.Errors.Any(e => e.Code == "guard.null")
            ? StatusCodes.Status404NotFound
            : StatusCodes.Status400BadRequest;

        return ToProblemResult("Unable to close project.", result, statusCode);
    }

    private static ProblemHttpResult ToProblemResult<T>(string title, CoreResults.Result<T> result, int statusCode = StatusCodes.Status400BadRequest)
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

    private static ProblemHttpResult ToProblemResult(string title, CoreResults.Result result, int statusCode = StatusCodes.Status400BadRequest)
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
