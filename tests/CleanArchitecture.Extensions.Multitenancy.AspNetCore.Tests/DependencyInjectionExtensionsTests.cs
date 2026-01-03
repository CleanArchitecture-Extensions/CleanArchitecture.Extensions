using CleanArchitecture.Extensions.Multitenancy.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Tests;

public class DependencyInjectionExtensionsTests
{
    [Fact]
    public void AddCleanArchitectureMultitenancyAspNetCore_registers_startup_filter_when_enabled()
    {
        var services = new ServiceCollection();

        services.AddCleanArchitectureMultitenancyAspNetCore(autoUseMiddleware: true);

        using var provider = services.BuildServiceProvider();

        var filters = provider.GetServices<IStartupFilter>();

        Assert.NotEmpty(filters);
    }
}
