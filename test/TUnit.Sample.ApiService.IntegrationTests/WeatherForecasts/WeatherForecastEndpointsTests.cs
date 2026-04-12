using System.Net;
using System.Net.Http.Json;
using TUnit.Sample.ApiService.Endpoints.WeatherForecasts;
using TUnit.Sample.ApiService.IntegrationTests.Utility;

namespace TUnit.Sample.ApiService.IntegrationTests.WeatherForecasts;

public class WeatherForecastEndpointsTests : CoreIntegrationTestBase
{
    [Test]
    public async Task GetWeatherForecast_ReturnsOk()
    {
        var client = Factory.CreateClient();
        var getResponse = await client.GetAsync("/weatherforecast");
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GetWeatherForecast_Returns_ValidJsonArray()
    {
        var client = Factory.CreateClient();
        var getResponse = await client.GetAsync("/weatherforecast");
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var responseContent = await getResponse.Content.ReadFromJsonAsync<WeatherForecastResponse[]>();
        
        await Assert.That(responseContent).IsNotNull();
        await Assert.That(responseContent.Length).IsGreaterThan(0);
    }
}