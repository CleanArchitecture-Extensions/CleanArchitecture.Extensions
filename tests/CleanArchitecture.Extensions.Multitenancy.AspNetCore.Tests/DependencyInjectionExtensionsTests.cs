using System.Linq;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.ApiExplorer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
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

    [Fact]
    public void AddCleanArchitectureMultitenancyAspNetCore_registers_api_description_provider_by_default()
    {
        var services = new ServiceCollection();

        services.AddCleanArchitectureMultitenancyAspNetCore();

        using var provider = services.BuildServiceProvider();

        var providers = provider.GetServices<IApiDescriptionProvider>().ToList();

        Assert.Contains(providers, item => item is TenantRouteApiDescriptionProvider);
    }
}
