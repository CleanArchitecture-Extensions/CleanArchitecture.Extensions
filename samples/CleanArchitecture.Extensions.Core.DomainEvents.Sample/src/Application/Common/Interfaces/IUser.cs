namespace CleanArchitecture.Extensions.Core.DomainEvents.Sample.Application.Common.Interfaces;

public interface IUser
{
    string? Id { get; }
    List<string>? Roles { get; }

}
