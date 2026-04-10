using System.Net;
using System.Net.Http.Json;
using TUnit.Sample.ApiService.IntegrationTests.Utility;

namespace TUnit.Sample.ApiService.IntegrationTests.Books;

public class BookEndpointTests
{
    [ClassDataSource<WebApplicationFactory>(Shared = SharedType.PerTestSession)]
    public required WebApplicationFactory WebApplicationFactory { get; init; }

    [Test]
    public async Task GetBooks_ReturnsOkWithSeededData()
    {
        var client = WebApplicationFactory.CreateClient();

        var response = await client.GetAsync("/books");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var books = await response.Content.ReadFromJsonAsync<List<BookResponse>>();
        await Assert.That(books).IsNotNull();
        await Assert.That(books!.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task PostBook_InvalidIsbn_ReturnsBadRequest()
    {
        var client = WebApplicationFactory.CreateClient();

        var request = new
        {
            Title = "Bad ISBN Book",
            Description = "A book with invalid ISBN",
            PublishDate = "2024-01-01T00:00:00Z",
            Isbn = "1234567890123",
            AuthorId = Guid.NewGuid()
        };

        var response = await client.PostAsJsonAsync("/books", request);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task PostBook_NonExistentAuthor_ReturnsBadRequest()
    {
        var client = WebApplicationFactory.CreateClient();

        var request = new
        {
            Title = "No Author Book",
            Description = "A book with non-existent author",
            PublishDate = "2024-01-01T00:00:00Z",
            Isbn = "9780306406157", // valid ISBN
            AuthorId = Guid.NewGuid()
        };

        var response = await client.PostAsJsonAsync("/books", request);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task PostAndGetBook_Roundtrip()
    {
        var client = WebApplicationFactory.CreateClient();

        // Get an existing author from seeded data
        var personsResponse = await client.GetAsync("/persons");
        var persons = await personsResponse.Content.ReadFromJsonAsync<List<PersonResponse>>();
        var authorId = persons![0].Id;

        var createRequest = new
        {
            Title = "Test Book",
            Description = "A test book",
            PublishDate = "2024-01-01T00:00:00Z",
            Isbn = "9780306406157",
            AuthorId = authorId
        };

        var postResponse = await client.PostAsJsonAsync("/books", createRequest);
        await Assert.That(postResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);

        var created = await postResponse.Content.ReadFromJsonAsync<IdResponse>();
        await Assert.That(created).IsNotNull();

        var getResponse = await client.GetAsync($"/books/{created!.Id}");
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var book = await getResponse.Content.ReadFromJsonAsync<BookDetailResponse>();
        await Assert.That(book).IsNotNull();
        await Assert.That(book!.Title).IsEqualTo("Test Book");
        await Assert.That(book.Isbn).IsEqualTo("9780306406157");
    }

    private record PersonResponse(Guid Id, string FirstName, string LastName);
    private record BookResponse(Guid Id, string Title, string Description, DateTime PublishDate, string Isbn, string FormattedIsbn, Guid AuthorId, string AuthorName);
    private record BookDetailResponse(Guid Id, string Title, string Description, DateTime PublishDate, string Isbn, string FormattedIsbn, Guid AuthorId, string AuthorName);
    private record IdResponse(Guid Id);
}
