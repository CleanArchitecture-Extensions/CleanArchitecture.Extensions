namespace CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Web.Infrastructure;

public abstract class EndpointGroupBase
{
    public virtual string? GroupName { get; }
    public abstract void Map(RouteGroupBuilder groupBuilder);
}
