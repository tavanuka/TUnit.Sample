using TUnit.Sample.ApiService.Endpoints.WeatherForecasts;

namespace TUnit.Sample.ApiService.Services;

public interface IWeatherForecastService
{
    WeatherForecastResponse[] GetForecast();
}