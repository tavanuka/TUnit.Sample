using TUnit.Sample.Common.Contracts.Books;
using TUnit.Sample.Common.Contracts.Persons;

namespace TUnit.Sample.ApiService.IntegrationTests.Books;

public class BookEndpointTests : CoreIntegrationTestBase
{
    // TODO: add more test cases for book endpoints, I.e. patch and put
    [Test]
    public async Task GetBooks_ReturnsOkWithSeededData()
    {
        await PopulateSchemaWithData();
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/books");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var books = await response.Content.ReadFromJsonAsync<List<BookResponse>>();
        await Assert.That(books).IsNotNull();
        await Assert.That(books.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task PostBook_InvalidIsbn_ReturnsBadRequest()
    {
        var client = Factory.CreateClient();

        var request = new CreateBookRequest(
            "Bad ISBN Book",
            "A book with invalid ISBN",
            DateTime.SpecifyKind(new DateTime(2024, 1, 1), DateTimeKind.Utc),
            "1234567890123",
            Guid.NewGuid());

        var response = await client.PostAsJsonAsync("/books", request);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task PostBook_NonExistentAuthor_ReturnsBadRequest()
    {
        var client = Factory.CreateClient();

        var request = new CreateBookRequest(
            "No Author Book",
            "A book with non-existent author",
            DateTime.SpecifyKind(new DateTime(2024, 1, 1), DateTimeKind.Utc),
            "9780306406157",
            Guid.NewGuid());

        var response = await client.PostAsJsonAsync("/books", request);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task PostAndGetBook_Roundtrip()
    {
        await PopulateSchemaWithData();
        var client = Factory.CreateClient();

        // Get an existing author from seeded data
        var personsResponse = await client.GetAsync("/persons");
        var persons = await personsResponse.Content.ReadFromJsonAsync<List<PersonResponse>>();

        await Assert.That(persons).IsNotNull();
        var authorId = persons[0].Id;

        var createRequest = new CreateBookRequest(
            "Test Book",
            "A test book",
            DateTime.SpecifyKind(new DateTime(2024, 1, 1), DateTimeKind.Utc),
            "9780306406157",
            authorId);

        var postResponse = await client.PostAsJsonAsync("/books", createRequest);
        await Assert.That(postResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);

        var created = await postResponse.Content.ReadFromJsonAsync<Guid>();
        await Assert.That(created).IsNotDefault();

        var getResponse = await client.GetAsync($"/books/{created}");
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var book = await getResponse.Content.ReadFromJsonAsync<BookDetailResponse>();
        await Assert.That(book).IsNotNull();
        await Assert.That(book.Title).IsEqualTo(createRequest.Title);
    }
}