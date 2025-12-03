using CleanArchitecture.Extensions.Core.Pipeline.Sample.Application.Diagnostics.Commands.SimulateWork;
using CleanArchitecture.Extensions.Core.Pipeline.Sample.Application.Diagnostics.Queries.GetPipelineDiagnostics;

namespace CleanArchitecture.Extensions.Core.Pipeline.Sample.Web.Endpoints;

public class Diagnostics : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetPipelineDiagnostics);
        groupBuilder.MapPost(SimulateWork, "simulate");
    }

    public async Task<IResult> GetPipelineDiagnostics(ISender sender)
    {
        var result = await sender.Send(new GetPipelineDiagnosticsQuery());
        return TypedResults.Ok(result);
    }

    public async Task<IResult> SimulateWork(ISender sender, int milliseconds = 600)
    {
        await sender.Send(new SimulateWorkCommand(milliseconds));
        return TypedResults.Accepted($"/api/{nameof(Diagnostics)}/simulate?milliseconds={milliseconds}");
    }
}
