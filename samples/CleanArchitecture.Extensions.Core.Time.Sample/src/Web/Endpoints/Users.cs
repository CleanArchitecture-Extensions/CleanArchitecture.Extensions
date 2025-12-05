using CleanArchitecture.Extensions.Core.Time.Sample.Infrastructure.Identity;

namespace CleanArchitecture.Extensions.Core.Time.Sample.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapIdentityApi<ApplicationUser>();
    }
}
