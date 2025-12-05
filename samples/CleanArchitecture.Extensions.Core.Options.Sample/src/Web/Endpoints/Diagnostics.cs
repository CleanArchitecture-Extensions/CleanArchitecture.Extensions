using CleanArchitecture.Extensions.Core.Options.Sample.Application.Diagnostics.Commands.EvaluateName;
using CleanArchitecture.Extensions.Core.Options.Sample.Application.Diagnostics.Queries.GetCoreOptions;
using Microsoft.AspNetCore.Http.HttpResults;
using CoreResults = CleanArchitecture.Extensions.Core.Results;

namespace CleanArchitecture.Extensions.Core.Options.Sample.Web.Endpoints;

public class Diagnostics : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetCoreOptions, "options");
        groupBuilder.MapPost(EvaluateName, "guard");
    }

    public async Task<Ok<CoreOptionsDto>> GetCoreOptions(ISender sender)
    {
        var options = await sender.Send(new GetCoreOptionsQuery());
        return TypedResults.Ok(options);
    }

    public async Task<Results<Ok<CoreResults.Result<string>>, BadRequest<CoreResults.Result<string>>>> EvaluateName(
        ISender sender,
        EvaluateNameCommand command)
    {
        var result = await sender.Send(command);
        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}
