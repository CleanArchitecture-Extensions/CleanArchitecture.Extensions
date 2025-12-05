using CleanArchitecture.Extensions.Core.Logging.Sample.Infrastructure.Identity;

namespace CleanArchitecture.Extensions.Core.Logging.Sample.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapIdentityApi<ApplicationUser>();
    }
}
