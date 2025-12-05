namespace CleanArchitecture.Extensions.Core.Options.Sample.Application.Common.Interfaces;

public interface IUser
{
    string? Id { get; }
    List<string>? Roles { get; }

}
