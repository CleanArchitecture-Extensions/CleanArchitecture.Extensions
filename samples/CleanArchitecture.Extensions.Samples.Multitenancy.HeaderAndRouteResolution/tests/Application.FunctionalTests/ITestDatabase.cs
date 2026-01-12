using System.Data.Common;

namespace CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Application.FunctionalTests;

public interface ITestDatabase
{
    Task InitialiseAsync();

    DbConnection GetConnection();

    string GetConnectionString();

    Task ResetAsync();

    Task DisposeAsync();
}
