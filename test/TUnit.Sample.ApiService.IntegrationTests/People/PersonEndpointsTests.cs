using TUnit.Sample.Common.Contracts.Persons;

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
        await Assert.That(persons.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task GetPersonById_InvalidId_ReturnsNotFound()
    {
        await PopulateSchemaWithData();
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/persons/{Guid.NewGuid()}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task PostAndGetPerson_Roundtrip()
    {
        var client = Factory.CreateClient();

        var createRequest = new CreatePersonRequest("Test", "Person", new DateTime(1990, 1, 15, 0, 0, 0, DateTimeKind.Utc));

        var postResponse = await client.PostAsJsonAsync("/persons", createRequest);
        await Assert.That(postResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);

        var created = await postResponse.Content.ReadFromJsonAsync<Guid>();
        await Assert.That(created).IsNotDefault();

        var getResponse = await client.GetAsync($"/persons/{created}");
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var person = await getResponse.Content.ReadFromJsonAsync<PersonDetailResponse>();
        await Assert.That(person).IsNotNull();
        await Assert.That(person.FirstName).IsEqualTo(createRequest.FirstName);
        await Assert.That(person.LastName).IsEqualTo(createRequest.LastName);
        await Assert.That(person.BirthDate).IsEqualTo(createRequest.BirthDate);
    }

    [Test]
    public async Task PatchPerson_ReturnsNoContent()
    {
        var client = Factory.CreateClient();

        // Create a person first
        var createRequest = new CreatePersonRequest("Before", "Update", DateTime.SpecifyKind(new DateTime(1995, 5, 5), DateTimeKind.Utc));
        var postResponse = await client.PostAsJsonAsync("/persons", createRequest);
        var created = await postResponse.Content.ReadFromJsonAsync<Guid>();
        await Assert.That(created).IsNotDefault();

        var updateRequest = createRequest with { FirstName = "After" };
        var putResponse = await client.PutAsJsonAsync($"/persons/{created}", updateRequest);

        await Assert.That(putResponse.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task PutPerson_ReturnsNoContent()
    {
        var client = Factory.CreateClient();

        // Create a person first
        var createRequest = new CreatePersonRequest("Before", "Update", DateTime.SpecifyKind(new DateTime(1995, 5, 5), DateTimeKind.Utc));
        var postResponse = await client.PostAsJsonAsync("/persons", createRequest);
        var created = await postResponse.Content.ReadFromJsonAsync<Guid>();
        await Assert.That(created).IsNotDefault();

        var updateRequest = createRequest with { FirstName = "After" };
        var putResponse = await client.PutAsJsonAsync($"/persons/{created}", updateRequest);

        await Assert.That(putResponse.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task PutPerson_InvalidId_ValidRequest_InsertsNewPerson()
    {
        var client = Factory.CreateClient();
        var createRequest = new CreatePersonRequest("Before", "Update", DateTime.SpecifyKind(new DateTime(1995, 5, 5), DateTimeKind.Utc));

        var putResponse = await client.PutAsJsonAsync($"/persons/{Guid.NewGuid()}", createRequest);
        await Assert.That(putResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);

        var created = await putResponse.Content.ReadFromJsonAsync<Guid>();
        await Assert.That(created).IsNotDefault();

        var getResponse = await client.GetAsync($"/persons/{created}");
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var getResponseContent = await getResponse.Content.ReadFromJsonAsync<PersonDetailResponse>();
        await Assert.That(getResponseContent).IsNotNull();

        await Assert.That(getResponseContent.FirstName).IsEqualTo(createRequest.FirstName);
        await Assert.That(getResponseContent.LastName).IsEqualTo(createRequest.LastName);
        await Assert.That(getResponseContent.BirthDate).IsEqualTo(createRequest.BirthDate);
    }

    [Test]
    public async Task PutPerson_InvalidId_InvalidRequest_ReturnsBadRequest()
    {
        var client = Factory.CreateClient();
        var createRequest = new UpdatePersonRequest(null, null, DateTime.SpecifyKind(new DateTime(1995, 5, 5), DateTimeKind.Utc));
        
        var putResponse = await client.PutAsJsonAsync($"/persons/{Guid.NewGuid()}", createRequest);
        await Assert.That(putResponse.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task DeletePerson_ThenGetReturnsNotFound()
    {
        var client = Factory.CreateClient();

        // Create a person first
        var createRequest = new CreatePersonRequest("Before", "Update", DateTime.SpecifyKind(new DateTime(1995, 1, 1), DateTimeKind.Utc));
        var postResponse = await client.PostAsJsonAsync("/persons", createRequest);
        var created = await postResponse.Content.ReadFromJsonAsync<Guid>();
        await Assert.That(created).IsNotDefault();

        var deleteResponse = await client.DeleteAsync($"/persons/{created}");
        await Assert.That(deleteResponse.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/persons/{created}");
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }
}