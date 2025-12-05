using CleanArchitecture.Extensions.Core.Time.Sample.Application.Diagnostics.Commands.SimulateDelay;
using CleanArchitecture.Extensions.Core.Time.Sample.Application.Diagnostics.Queries.GetTimeSnapshot;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CleanArchitecture.Extensions.Core.Time.Sample.Web.Endpoints;

public sealed class Diagnostics : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetTimeSnapshot, "time");
        groupBuilder.MapPost(SimulateDelay, "delay");
    }

    public async Task<Ok<TimeSnapshotDto>> GetTimeSnapshot(ISender sender)
    {
        var snapshot = await sender.Send(new GetTimeSnapshotQuery());
        return TypedResults.Ok(snapshot);
    }

    public async Task<Ok<DelayResult>> SimulateDelay(ISender sender, SimulateDelayCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }
}
