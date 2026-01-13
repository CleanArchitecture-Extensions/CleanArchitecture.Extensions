using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Middleware;

/// <summary>
/// Ensures the default exception handler middleware is registered so <see cref="IExceptionHandler"/> implementations run.
/// </summary>
internal sealed class ExceptionHandlerStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        ArgumentNullException.ThrowIfNull(next);

        return app =>
        {
            app.UseExceptionHandler();
            next(app);
        };
    }
}
