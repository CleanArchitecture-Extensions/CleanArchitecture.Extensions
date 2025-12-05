using System.Data.Common;

namespace CleanArchitecture.Extensions.Core.Logging.Sample.Application.FunctionalTests;

public interface ITestDatabase
{
    Task InitialiseAsync();

    DbConnection GetConnection();

    string GetConnectionString();

    Task ResetAsync();

    Task DisposeAsync();
}
