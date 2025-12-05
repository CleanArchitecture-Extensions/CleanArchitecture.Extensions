using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Infrastructure.Identity;

namespace CleanArchitecture.Extensions.Core.DomainEvents.Sample.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapIdentityApi<ApplicationUser>();
    }
}
