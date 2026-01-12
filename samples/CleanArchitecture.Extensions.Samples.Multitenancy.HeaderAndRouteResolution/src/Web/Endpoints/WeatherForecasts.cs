using CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Application.WeatherForecasts.Queries.GetWeatherForecasts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Web.Endpoints;

public class WeatherForecasts : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetWeatherForecasts);
    }

    public async Task<Ok<IEnumerable<WeatherForecast>>> GetWeatherForecasts(ISender sender)
    {
        var forecasts = await sender.Send(new GetWeatherForecastsQuery());

        return TypedResults.Ok(forecasts);
    }

}
