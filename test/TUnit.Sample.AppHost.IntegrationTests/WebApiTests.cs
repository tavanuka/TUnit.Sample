using TUnit.Sample.Common.Constants;

namespace TUnit.Sample.AppHost.IntegrationTests;

[ClassDataSource<AppFixture>(Shared = SharedType.PerTestSession)]
public class WebApiTests(AppFixture fixture)
{
    [Test]
    public async Task GetWeatherForecast_Returns_StatusCode_OK()
    {
        var client = fixture.CreateHttpClient(ResourceConstants.WebApi);
        var response = await client.GetAsync("/weatherforecast");
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }
}
  