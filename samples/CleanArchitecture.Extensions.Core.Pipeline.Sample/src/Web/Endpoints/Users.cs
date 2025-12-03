using CleanArchitecture.Extensions.Core.Pipeline.Sample.Infrastructure.Identity;

namespace CleanArchitecture.Extensions.Core.Pipeline.Sample.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapIdentityApi<ApplicationUser>();
    }
}
