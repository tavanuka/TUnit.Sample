using TUnit.Sample.ApiService.Services;
using TUnit.Sample.Common.Contracts.Books;

namespace TUnit.Sample.ApiService.Endpoints.Books;

public static class BooksEndpoints
{
    public static void MapBooksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/books");

        group.MapGet("/", async (IBookService bookService, CancellationToken ct)
            => Results.Ok(await bookService.GetAll(ct))
        );

        group.MapGet("/{id:guid}", async (Guid id, IBookService bookService, CancellationToken ct) => {
            var book = await bookService.GetById(id, ct);
            return book is null
                ? Results.NotFound()
                : Results.Ok(book);
        });

        group.MapPost("/", async (CreateBookRequest request, IBookService bookService, CancellationToken ct) => {
            var result = await bookService.Insert(request, ct);

            return result is null
                ? Results.BadRequest("Invalid request. Please check the request body.")
                : Results.Created($"/books/{result}", result);
        });

        group.MapPatch("/{id:guid}", async (Guid id, UpdateBookRequest request, IBookService bookService, CancellationToken ct) => {
            var updated = await bookService.Update(id, request, ct);
            return updated
                ? Results.NoContent()
                : Results.NotFound();
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateBookRequest request, IBookService bookService, CancellationToken ct) => {
            var updated = await bookService.Update(id, request, ct);
            if (updated)
                return Results.NoContent();

            // TODO: specify FluentValidation rules instead 
            if (!request.PublishDate.HasValue
                || string.IsNullOrWhiteSpace(request.Title)
                || string.IsNullOrWhiteSpace(request.Description)
                || string.IsNullOrWhiteSpace(request.Isbn)
                || !request.AuthorId.HasValue)
                return Results.BadRequest("Book could not be found or invalid data provided.");

            var createRequest = new CreateBookRequest(request.Title, request.Description, request.PublishDate.Value, request.Isbn, request.AuthorId.Value);

            return await bookService.Insert(createRequest, ct) is {} createdId
                ? Results.Created($"/books/{createdId}", createdId)
                : Results.BadRequest();
        });

        group.MapDelete("/{id:guid}", async (Guid id, IBookService bookService, CancellationToken ct) => {
            var deleted = await bookService.Delete(id, ct);
            return deleted
                ? Results.NoContent()
                : Results.NotFound();
        });
    }
}