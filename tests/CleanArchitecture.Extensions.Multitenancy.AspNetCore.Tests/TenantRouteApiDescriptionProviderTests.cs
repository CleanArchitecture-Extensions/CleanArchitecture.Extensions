using CleanArchitecture.Extensions.Multitenancy.AspNetCore.ApiExplorer;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Options;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Tests;

public class TenantRouteApiDescriptionProviderTests
{
    [Fact]
    public void OnProvidersExecuting_inserts_tenant_route_parameter_when_missing()
    {
        var options = OptionsFactory.Create(new MultitenancyOptions { RouteParameterName = "tenantId" });
        var aspNetCoreOptions = OptionsFactory.Create(new AspNetCoreMultitenancyOptions { EnableOpenApiIntegration = true });
        var provider = new TenantRouteApiDescriptionProvider(options, aspNetCoreOptions, new EmptyModelMetadataProvider());

        var actionDescriptor = new ActionDescriptor
        {
            AttributeRouteInfo = new AttributeRouteInfo
            {
                Template = "api/tenants/{tenantId}/TodoItems"
            }
        };

        var description = new ApiDescription
        {
            ActionDescriptor = actionDescriptor,
            RelativePath = "api/tenants/TodoItems"
        };

        var context = new ApiDescriptionProviderContext(new[] { actionDescriptor });
        context.Results.Add(description);

        provider.OnProvidersExecuting(context);

        Assert.Equal("api/tenants/{tenantId}/TodoItems", description.RelativePath);
        var parameter = Assert.Single(description.ParameterDescriptions, item =>
            string.Equals(item.Name, "tenantId", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(BindingSource.Path, parameter.Source);
        Assert.True(parameter.IsRequired);
    }

    [Fact]
    public void OnProvidersExecuting_noops_when_openapi_integration_disabled()
    {
        var options = OptionsFactory.Create(new MultitenancyOptions { RouteParameterName = "tenantId" });
        var aspNetCoreOptions = OptionsFactory.Create(new AspNetCoreMultitenancyOptions { EnableOpenApiIntegration = false });
        var provider = new TenantRouteApiDescriptionProvider(options, aspNetCoreOptions, new EmptyModelMetadataProvider());

        var actionDescriptor = new ActionDescriptor
        {
            AttributeRouteInfo = new AttributeRouteInfo
            {
                Template = "api/tenants/{tenantId}/TodoItems"
            }
        };

        var description = new ApiDescription
        {
            ActionDescriptor = actionDescriptor,
            RelativePath = "api/tenants/TodoItems"
        };

        var context = new ApiDescriptionProviderContext(new[] { actionDescriptor });
        context.Results.Add(description);

        provider.OnProvidersExecuting(context);

        Assert.Equal("api/tenants/TodoItems", description.RelativePath);
        Assert.Empty(description.ParameterDescriptions);
    }

    [Fact]
    public void OnProvidersExecuting_uses_route_pattern_when_attribute_template_missing()
    {
        var options = OptionsFactory.Create(new MultitenancyOptions { RouteParameterName = "tenantId" });
        var aspNetCoreOptions = OptionsFactory.Create(new AspNetCoreMultitenancyOptions { EnableOpenApiIntegration = true });
        var provider = new TenantRouteApiDescriptionProvider(options, aspNetCoreOptions, new EmptyModelMetadataProvider());

        var actionDescriptor = new StubRouteActionDescriptor
        {
            RoutePattern = new StubRoutePattern("api/tenants/{tenantId}/TodoItems")
        };

        var description = new ApiDescription
        {
            ActionDescriptor = actionDescriptor,
            RelativePath = "api/tenants/TodoItems"
        };

        var context = new ApiDescriptionProviderContext(new[] { actionDescriptor });
        context.Results.Add(description);

        provider.OnProvidersExecuting(context);

        Assert.Equal("api/tenants/{tenantId}/TodoItems", description.RelativePath);
        var parameter = Assert.Single(description.ParameterDescriptions, item =>
            string.Equals(item.Name, "tenantId", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(BindingSource.Path, parameter.Source);
    }

    private sealed class StubRoutePattern
    {
        public StubRoutePattern(string rawText)
        {
            RawText = rawText;
        }

        public string RawText { get; }
    }

    private sealed class StubRouteActionDescriptor : ActionDescriptor
    {
        public StubRoutePattern? RoutePattern { get; set; }
    }

    [Fact]
    public void OnProvidersExecuting_uses_endpoint_metadata_route_pattern_when_available()
    {
        var options = OptionsFactory.Create(new MultitenancyOptions { RouteParameterName = "tenantId" });
        var aspNetCoreOptions = OptionsFactory.Create(new AspNetCoreMultitenancyOptions { EnableOpenApiIntegration = true });
        var provider = new TenantRouteApiDescriptionProvider(options, aspNetCoreOptions, new EmptyModelMetadataProvider());

        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata = new List<object>
            {
                new StubEndpointMetadata(new StubRoutePattern("api/tenants/{tenantId}/TodoItems"))
            }
        };

        var description = new ApiDescription
        {
            ActionDescriptor = actionDescriptor,
            RelativePath = "api/tenants/TodoItems"
        };

        var context = new ApiDescriptionProviderContext(new[] { actionDescriptor });
        context.Results.Add(description);

        provider.OnProvidersExecuting(context);

        Assert.Equal("api/tenants/{tenantId}/TodoItems", description.RelativePath);
        var parameter = Assert.Single(description.ParameterDescriptions, item =>
            string.Equals(item.Name, "tenantId", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(BindingSource.Path, parameter.Source);
    }

    private sealed class StubEndpointMetadata
    {
        public StubEndpointMetadata(StubRoutePattern routePattern)
        {
            RoutePattern = routePattern;
        }

        public StubRoutePattern RoutePattern { get; }
    }

    [Fact]
    public void OnProvidersExecuting_uses_display_name_route_when_other_sources_missing()
    {
        var options = OptionsFactory.Create(new MultitenancyOptions { RouteParameterName = "tenantId" });
        var aspNetCoreOptions = OptionsFactory.Create(new AspNetCoreMultitenancyOptions { EnableOpenApiIntegration = true });
        var provider = new TenantRouteApiDescriptionProvider(options, aspNetCoreOptions, new EmptyModelMetadataProvider());

        var actionDescriptor = new ActionDescriptor
        {
            DisplayName = "HTTP: GET /api/tenants/{tenantId}/TodoItems"
        };

        var description = new ApiDescription
        {
            ActionDescriptor = actionDescriptor,
            RelativePath = "api/tenants/TodoItems"
        };

        var context = new ApiDescriptionProviderContext(new[] { actionDescriptor });
        context.Results.Add(description);

        provider.OnProvidersExecuting(context);

        Assert.Equal("api/tenants/{tenantId}/TodoItems", description.RelativePath);
        var parameter = Assert.Single(description.ParameterDescriptions, item =>
            string.Equals(item.Name, "tenantId", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(BindingSource.Path, parameter.Source);
    }
}
