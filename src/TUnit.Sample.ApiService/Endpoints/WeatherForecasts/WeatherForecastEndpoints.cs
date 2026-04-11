using TUnit.Sample.ApiService.Services;

namespace TUnit.Sample.ApiService.Endpoints.WeatherForecasts;

public record WeatherForecastResponse(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public static class WeatherForecastEndpoints
{
    public static void MapWeatherForecastEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/weatherforecast", (IWeatherForecastService forecastService) =>  Results.Ok(forecastService.GetForecast()))
            .WithName("GetWeatherForecast");

    }
}