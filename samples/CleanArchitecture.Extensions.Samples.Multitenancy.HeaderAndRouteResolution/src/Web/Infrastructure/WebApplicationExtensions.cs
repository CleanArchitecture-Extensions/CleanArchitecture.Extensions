using System.Reflection;
// Step 6: (Begin) Tenant enforcement routing helpers
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Routing;
// Step 6: (End) Tenant enforcement routing helpers

namespace CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Web.Infrastructure;

public static class WebApplicationExtensions
{
    private static RouteGroupBuilder MapGroup(this WebApplication app, EndpointGroupBase group)
    {
        var groupName = group.GroupName ?? group.GetType().Name;

        // Step 5: (Begin) Prefix tenant-bound endpoints with tenant route
        var tenantRoutePrefix = "/api/tenants/{tenantId}";

        var routeGroup = app
            .MapGroup($"{tenantRoutePrefix}/{groupName}")
            .WithGroupName(groupName)
            .WithTags(groupName);
        // Step 5: (End) Prefix tenant-bound endpoints with tenant route

        // Step 6: (Begin) Enforce tenant requirements for grouped endpoints
        routeGroup.AddTenantEnforcement();
        routeGroup.RequireTenant();
        // Step 6: (End) Enforce tenant requirements for grouped endpoints

        return routeGroup;
    }

    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var endpointGroupType = typeof(EndpointGroupBase);

        var assembly = Assembly.GetExecutingAssembly();

        var endpointGroupTypes = assembly.GetExportedTypes()
            .Where(t => t.IsSubclassOf(endpointGroupType));

        foreach (var type in endpointGroupTypes)
        {
            if (Activator.CreateInstance(type) is EndpointGroupBase instance)
            {
                instance.Map(app.MapGroup(instance));
            }
        }

        return app;
    }
}
