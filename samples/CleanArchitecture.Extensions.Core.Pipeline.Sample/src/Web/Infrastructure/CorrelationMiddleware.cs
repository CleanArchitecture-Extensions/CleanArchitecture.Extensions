using CleanArchitecture.Extensions.Core.Logging;
using CleanArchitecture.Extensions.Core.Options;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Core.Pipeline.Sample.Web.Infrastructure;

public sealed class CorrelationMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ILogContext logContext, IOptions<CoreExtensionsOptions> options)
    {
        var headerName = options.Value.CorrelationHeaderName;
        var incoming = context.Request.Headers[headerName].FirstOrDefault();
        var correlationId = string.IsNullOrWhiteSpace(incoming)
            ? options.Value.CorrelationIdFactory()
            : incoming;

        logContext.CorrelationId = correlationId;

        using var scope = logContext.PushProperty("CorrelationId", correlationId);
        context.Response.Headers[headerName] = correlationId;

        await _next(context);
    }
}
