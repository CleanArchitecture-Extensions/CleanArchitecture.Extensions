using CleanArchitecture.Extensions.Core.Logging.Sample.Application.Diagnostics.Commands.EmitLogPulse;
using CleanArchitecture.Extensions.Core.Logging.Sample.Application.Diagnostics.Queries.GetRecentLogs;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CleanArchitecture.Extensions.Core.Logging.Sample.Web.Endpoints;

public sealed class Diagnostics : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(EmitLogPulse, "pulse");
        groupBuilder.MapGet(GetRecentLogs, "logs");
    }

    public async Task<Results<Accepted, Ok>> EmitLogPulse(ISender sender, EmitLogPulseCommand command)
    {
        await sender.Send(command);
        return TypedResults.Accepted($"/api/{nameof(Diagnostics)}/logs");
    }

    public async Task<Ok<IReadOnlyCollection<CleanArchitecture.Extensions.Core.Logging.LogEntry>>> GetRecentLogs(
        ISender sender,
        [AsParameters] GetRecentLogsQuery query)
    {
        var entries = await sender.Send(query);
        return TypedResults.Ok(entries);
    }
}
