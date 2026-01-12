using System.Reflection;

namespace CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Web.Infrastructure;

public static class WebApplicationExtensions
{
    private static RouteGroupBuilder MapGroup(this WebApplication app, EndpointGroupBase group)
    {
        var groupName = group.GroupName ?? group.GetType().Name;

        // Step 5: (Begin) Prefix tenant-bound endpoints with tenant route
        var tenantRoutePrefix = "/api/tenants/{tenantId}";

        return app
            .MapGroup($"{tenantRoutePrefix}/{groupName}")
            .WithGroupName(groupName)
            .WithTags(groupName);
        // Step 5: (End) Prefix tenant-bound endpoints with tenant route
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
