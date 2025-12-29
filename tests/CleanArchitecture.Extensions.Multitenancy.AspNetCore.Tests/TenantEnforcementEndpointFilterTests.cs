using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Filters;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using CleanArchitecture.Extensions.Multitenancy.Context;
using Microsoft.AspNetCore.Http;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Tests;

public class TenantEnforcementEndpointFilterTests
{
    [Fact]
    public async Task InvokeAsync_returns_problem_when_tenant_required_and_missing()
    {
        var currentTenant = new CurrentTenantAccessor();
        var filter = new TenantEnforcementEndpointFilter(currentTenant, OptionsFactory.Create(new MultitenancyOptions()));

        var httpContext = new DefaultHttpContext();
        var endpoint = new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(new RequiresTenantAttribute()),
            "test");
        httpContext.SetEndpoint(endpoint);

        var invocationContext = EndpointFilterInvocationContext.Create(httpContext);

        var result = await filter.InvokeAsync(invocationContext, _ => ValueTask.FromResult<object?>(Results.Ok()));

        Assert.NotNull(result);
        await ((IResult)result!).ExecuteAsync(httpContext);
        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
    }
}
