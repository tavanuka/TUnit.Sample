using TUnit.Sample.ApiService.Endpoints.WeatherForecasts;

namespace TUnit.Sample.ApiService.Services;

public class WeatherForecastService : IWeatherForecastService
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public WeatherForecastResponse[] GetForecast() => Enumerable.Range(1, 5).Select(index =>
            new WeatherForecastResponse
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                Summaries[Random.Shared.Next(Summaries.Length)]
            ))
        .ToArray();
}