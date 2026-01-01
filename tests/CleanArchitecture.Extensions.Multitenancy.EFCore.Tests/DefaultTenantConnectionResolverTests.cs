using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Factories;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Tests;

public class DefaultTenantConnectionResolverTests
{
    [Fact]
    public void ResolveConnectionString_uses_format_from_options()
    {
        var options = new EfCoreMultitenancyOptions
        {
            ConnectionStringFormat = "Server={0};"
        };

        var resolver = new DefaultTenantConnectionResolver(Microsoft.Extensions.Options.Options.Create(options));

        var connectionString = resolver.ResolveConnectionString(new TenantInfo("alpha"));

        Assert.Equal("Server=alpha;", connectionString);
    }

    [Fact]
    public void ResolveConnectionString_uses_provider_from_options()
    {
        var options = new EfCoreMultitenancyOptions
        {
            ConnectionStringProvider = _ => "provider"
        };

        var resolver = new DefaultTenantConnectionResolver(Microsoft.Extensions.Options.Options.Create(options));

        var connectionString = resolver.ResolveConnectionString(new TenantInfo("alpha"));

        Assert.Equal("provider", connectionString);
    }
}
