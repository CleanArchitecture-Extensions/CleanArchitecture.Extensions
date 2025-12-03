namespace CleanArchitecture.Extensions.Core.Pipeline.Sample.Application.Common.Interfaces;

public interface IUser
{
    string? Id { get; }
    List<string>? Roles { get; }

}
