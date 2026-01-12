namespace CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Application.Common.Interfaces;

public interface IUser
{
    string? Id { get; }
    List<string>? Roles { get; }

}
