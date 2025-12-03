namespace CleanArchitecture.Extensions.Core.Result.Sample.Application.Common.Interfaces;

public interface IUser
{
    string? Id { get; }
    List<string>? Roles { get; }

}
