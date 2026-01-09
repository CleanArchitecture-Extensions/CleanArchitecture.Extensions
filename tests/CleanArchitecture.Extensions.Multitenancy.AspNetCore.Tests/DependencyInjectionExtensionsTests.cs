using System.Linq;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Tests;

public class DependencyInjectionExtensionsTests
{
    [Fact]
    public void AddCleanArchitectureMultitenancyAspNetCore_registers_exception_handler_startup_filter_by_default()
    {
        var services = new ServiceCollection();

        services.AddCleanArchitectureMultitenancyAspNetCore();

        using var provider = services.BuildServiceProvider();

        var filters = provider.GetServices<IStartupFilter>().ToList();

        Assert.Contains(filters, filter => filter.GetType().Name == "ExceptionHandlerStartupFilter");
    }

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
