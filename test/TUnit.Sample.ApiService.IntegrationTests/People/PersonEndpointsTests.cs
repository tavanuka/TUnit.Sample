using System.Net;
using System.Net.Http.Json;
using TUnit.Sample.ApiService.IntegrationTests.Utility;

namespace TUnit.Sample.ApiService.IntegrationTests.People;

public class PersonEndpointsTests : CoreIntegrationTestBase
{

    [Test]
    public async Task GetPersons_ReturnsOkWithSeededData()
    {
        await PopulateSchemaWithData();
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/persons");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var persons = await response.Content.ReadFromJsonAsync<List<PersonResponse>>();
        await Assert.That(persons).IsNotNull();
        await Assert.That(persons!.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task GetPersonById_InvalidId_ReturnsNotFound()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/persons/{Guid.NewGuid()}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task PostAndGetPerson_Roundtrip()
    {
        var client = Factory.CreateClient();

        var createRequest = new
        {
            FirstName = "Test",
            LastName = "Person",
            BirthDate = "2000-01-15T00:00:00Z"
        };

        var postResponse = await client.PostAsJsonAsync("/persons", createRequest);
        await Assert.That(postResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);

        var created = await postResponse.Content.ReadFromJsonAsync<IdResponse>();
        await Assert.That(created).IsNotNull();

        var getResponse = await client.GetAsync($"/persons/{created!.Id}");
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var person = await getResponse.Content.ReadFromJsonAsync<PersonDetailResponse>();
        await Assert.That(person).IsNotNull();
        await Assert.That(person!.FirstName).IsEqualTo("Test");
        await Assert.That(person.LastName).IsEqualTo("Person");
    }

    [Test]
    public async Task PutPerson_ReturnsNoContent()
    {
        var client = Factory.CreateClient();

        // Create a person first
        var createRequest = new { FirstName = "Before", LastName = "Update", BirthDate = "1995-05-05T00:00:00Z" };
        var postResponse = await client.PostAsJsonAsync("/persons", createRequest);
        var created = await postResponse.Content.ReadFromJsonAsync<IdResponse>();

        var updateRequest = new { FirstName = "After", LastName = "Update", BirthDate = "1995-05-05T00:00:00Z" };
        var putResponse = await client.PutAsJsonAsync($"/persons/{created!.Id}", updateRequest);

        await Assert.That(putResponse.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task DeletePerson_ThenGetReturnsNotFound()
    {
        var client = Factory.CreateClient();

        // Create a person first
        var createRequest = new { FirstName = "ToDelete", LastName = "Person", BirthDate = "1990-01-01T00:00:00Z" };
        var postResponse = await client.PostAsJsonAsync("/persons", createRequest);
        var created = await postResponse.Content.ReadFromJsonAsync<IdResponse>();

        var deleteResponse = await client.DeleteAsync($"/persons/{created!.Id}");
        await Assert.That(deleteResponse.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/persons/{created.Id}");
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    private record PersonResponse(Guid Id, string FirstName, string LastName, DateTime BirthDate, int Age, bool IsMinor);
    private record PersonDetailResponse(Guid Id, string FirstName, string LastName, DateTime BirthDate, int Age, bool IsMinor, List<PersonBookResponse> AuthoredBooks);
    private record PersonBookResponse(Guid Id, string Title, string Isbn);
    private record IdResponse(Guid Id);
}
