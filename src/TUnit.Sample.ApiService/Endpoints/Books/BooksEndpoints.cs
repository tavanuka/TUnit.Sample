using Microsoft.EntityFrameworkCore;
using TUnit.Sample.ApiService.Services;
using TUnit.Sample.Infrastructure;

namespace TUnit.Sample.ApiService.Endpoints.Books;

public record BookResponse(Guid Id, string Title, string Description, DateTime PublishDate, string Isbn, string FormattedIsbn, Guid AuthorId, string AuthorName);
public record BookDetailResponse(Guid Id, string Title, string Description, DateTime PublishDate, string Isbn, string FormattedIsbn, Guid AuthorId, string AuthorName);
public record CreateBookRequest(string Title, string Description, DateTime PublishDate, string Isbn, Guid AuthorId);
public record UpdateBookRequest(string Title, string Description, DateTime PublishDate, string Isbn, Guid AuthorId);

public static class BooksEndpoints
{
    public static void MapBooksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/books");

        group.MapGet("/", async (CoreDbContext db, BookService bookService) =>
        {
            var books = await db.Books
                .AsNoTracking()
                .Include(b => b.Author)
                .ToListAsync();

            return Results.Ok(books.Select(b => new BookResponse(
                b.Id, b.Title, b.Description, b.PublishDate,
                b.Isbn, bookService.FormatIsbn(b.Isbn),
                b.AuthorId, b.Author.FullName
            )));
        });

        group.MapGet("/{id:guid}", async (Guid id, CoreDbContext db, BookService bookService) =>
        {
            var book = await db.Books
                .AsNoTracking()
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book is null)
                return Results.NotFound();

            return Results.Ok(new BookDetailResponse(
                book.Id, book.Title, book.Description, book.PublishDate,
                book.Isbn, bookService.FormatIsbn(book.Isbn),
                book.AuthorId, book.Author.FullName
            ));
        });

        group.MapPost("/", async (CreateBookRequest request, CoreDbContext db, BookService bookService) =>
        {
            if (!bookService.ValidateIsbn13(request.Isbn))
                return Results.BadRequest(new { Error = "Invalid ISBN-13" });

            var authorExists = await db.Persons.AnyAsync(p => p.Id == request.AuthorId);
            if (!authorExists)
                return Results.BadRequest(new { Error = "Author not found" });

            var book = new Domain.Book
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                PublishDate = request.PublishDate,
                Isbn = request.Isbn,
                AuthorId = request.AuthorId
            };

            db.Books.Add(book);
            await db.SaveChangesAsync();

            return Results.Created($"/books/{book.Id}", new { book.Id });
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateBookRequest request, CoreDbContext db, BookService bookService) =>
        {
            if (!bookService.ValidateIsbn13(request.Isbn))
                return Results.BadRequest(new { Error = "Invalid ISBN-13" });

            var book = await db.Books.FindAsync(id);
            if (book is null)
                return Results.NotFound();

            book.Title = request.Title;
            book.Description = request.Description;
            book.PublishDate = request.PublishDate;
            book.Isbn = request.Isbn;
            book.AuthorId = request.AuthorId;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapDelete("/{id:guid}", async (Guid id, CoreDbContext db) =>
        {
            var book = await db.Books.FindAsync(id);
            if (book is null)
                return Results.NotFound();

            db.Books.Remove(book);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
