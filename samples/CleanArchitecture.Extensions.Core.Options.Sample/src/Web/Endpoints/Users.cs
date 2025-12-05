using CleanArchitecture.Extensions.Core.Options.Sample.Infrastructure.Identity;

namespace CleanArchitecture.Extensions.Core.Options.Sample.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapIdentityApi<ApplicationUser>();
    }
}
