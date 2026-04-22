using TUnit.Sample.ApiService.Services;

namespace TUnit.Sample.ApiService.Endpoints.WeatherForecasts;

public static class WeatherForecastEndpoints
{
    public static void MapWeatherForecastEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/weatherforecast", (IWeatherForecastService forecastService) =>  Results.Ok(forecastService.GetForecast()))
            .WithName("GetWeatherForecast");

    }
}