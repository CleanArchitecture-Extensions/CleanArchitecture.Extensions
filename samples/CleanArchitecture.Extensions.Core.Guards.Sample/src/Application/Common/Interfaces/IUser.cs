namespace CleanArchitecture.Extensions.Core.Guards.Sample.Application.Common.Interfaces;

public interface IUser
{
    string? Id { get; }
    List<string>? Roles { get; }

}
