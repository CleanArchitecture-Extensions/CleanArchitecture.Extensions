namespace CleanArchitecture.Extensions.Core.Logging.Sample.Application.Common.Interfaces;

public interface IUser
{
    string? Id { get; }
    List<string>? Roles { get; }

}
