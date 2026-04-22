using TUnit.Sample.Common.Contracts.WeatherForecasts;

namespace TUnit.Sample.ApiService.Services;

public interface IWeatherForecastService
{
    WeatherForecastResponse[] GetForecast();
}