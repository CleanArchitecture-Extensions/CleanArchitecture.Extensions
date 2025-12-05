using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Application.Common.Interfaces;
using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Application.Diagnostics.Queries.GetRecentDomainEvents;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CleanArchitecture.Extensions.Core.DomainEvents.Sample.Web.Endpoints;

public class DomainEvents : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetRecentDomainEvents, "recent");
    }

    public async Task<Ok<IReadOnlyCollection<DomainEventLogEntry>>> GetRecentDomainEvents(
        ISender sender,
        [AsParameters] GetRecentDomainEventsQuery query)
    {
        var entries = await sender.Send(query);
        return TypedResults.Ok(entries);
    }
}
