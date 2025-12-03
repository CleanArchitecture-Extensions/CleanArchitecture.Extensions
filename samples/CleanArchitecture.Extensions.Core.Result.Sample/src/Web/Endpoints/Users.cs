using CleanArchitecture.Extensions.Core.Result.Sample.Infrastructure.Identity;

namespace CleanArchitecture.Extensions.Core.Result.Sample.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapIdentityApi<ApplicationUser>();
    }
}
