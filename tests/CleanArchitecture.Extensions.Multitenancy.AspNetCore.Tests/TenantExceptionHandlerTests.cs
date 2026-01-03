using CleanArchitecture.Extensions.Multitenancy.AspNetCore.ProblemDetails;
using CleanArchitecture.Extensions.Multitenancy.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Tests;

public class TenantExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_maps_multitenancy_exceptions()
    {
        var handler = new TenantExceptionHandler();
        var httpContext = new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream()
            }
        };
        httpContext.RequestServices = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        var handled = await handler.TryHandleAsync(
            httpContext,
            new TenantNotResolvedException(),
            CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_skips_unmapped_exceptions()
    {
        var handler = new TenantExceptionHandler();
        var httpContext = new DefaultHttpContext();

        var handled = await handler.TryHandleAsync(
            httpContext,
            new InvalidOperationException("unexpected"),
            CancellationToken.None);

        Assert.False(handled);
    }
}
