using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Middleware;

internal sealed class TenantResolutionStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        ArgumentNullException.ThrowIfNull(next);

        return app =>
        {
            app.UseCleanArchitectureMultitenancy();
            next(app);
        };
    }
}
