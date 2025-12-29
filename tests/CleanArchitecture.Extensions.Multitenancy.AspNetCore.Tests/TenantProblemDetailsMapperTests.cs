using CleanArchitecture.Extensions.Multitenancy.AspNetCore.ProblemDetails;
using CleanArchitecture.Extensions.Multitenancy.Exceptions;
using Microsoft.AspNetCore.Http;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Tests;

public class TenantProblemDetailsMapperTests
{
    [Theory]
    [InlineData(typeof(TenantNotResolvedException), StatusCodes.Status400BadRequest)]
    [InlineData(typeof(TenantNotFoundException), StatusCodes.Status404NotFound)]
    [InlineData(typeof(TenantSuspendedException), StatusCodes.Status403Forbidden)]
    [InlineData(typeof(TenantInactiveException), StatusCodes.Status403Forbidden)]
    public void TryCreate_maps_exceptions(Type exceptionType, int expectedStatus)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/test";

        Exception exception = exceptionType switch
        {
            var type when type == typeof(TenantNotFoundException) => new TenantNotFoundException("tenant-1"),
            var type when type == typeof(TenantSuspendedException) => new TenantSuspendedException("tenant-1"),
            var type when type == typeof(TenantInactiveException) => new TenantInactiveException("tenant-1"),
            _ => new TenantNotResolvedException()
        };

        var mapped = TenantProblemDetailsMapper.TryCreate(exception, httpContext, out var details);

        Assert.True(mapped);
        Assert.Equal(expectedStatus, details.Status);
        Assert.Equal("/test", details.Instance);
    }
}
