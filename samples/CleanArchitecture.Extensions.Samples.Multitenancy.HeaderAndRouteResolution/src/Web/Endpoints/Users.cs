using CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Infrastructure.Identity;

namespace CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapIdentityApi<ApplicationUser>();
    }
}
