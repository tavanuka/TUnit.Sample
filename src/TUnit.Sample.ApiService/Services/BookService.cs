using Microsoft.EntityFrameworkCore;
using TUnit.Sample.ApiService.Endpoints.Books;
using TUnit.Sample.Domain;
using TUnit.Sample.Infrastructure;

namespace TUnit.Sample.ApiService.Services;

public class BookService(CoreDbContext context, IIsbnFormatter formatter) : IBookService
{
    public async Task<List<BookResponse>> GetAll(CancellationToken cancellationToken = default)
    {
        var books = await context.Books
            .AsNoTracking()
            .Select(b => new BookResponse(
                b.Id,
                b.Title,
                b.Description,
                b.PublishDate,
                b.Isbn, formatter.FormatIsbn(b.Isbn),
                b.AuthorId,
                b.Author.FullName))
            .ToListAsync(cancellationToken: cancellationToken);

        return books;
    }

    public async Task<BookDetailResponse?> GetById(Guid id, CancellationToken cancellationToken = default)
        => await context.Books
            .AsNoTracking()
            .Where(b => b.Id == id)
            .Select(b => new BookDetailResponse(
                b.Id,
                b.Title,
                b.Description,
                b.PublishDate,
                b.Isbn, formatter.FormatIsbn(b.Isbn),
                b.AuthorId,
                b.Author.FullName))
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

    public async Task<Guid?> Insert(CreateBookRequest request, CancellationToken cancellationToken = default)
    {
        // TODO: Fluent Validation and better result management
        if (!formatter.ValidateIsbn13(request.Isbn))
            return null;

        if (!await context.Persons.AnyAsync(x => x.Id == request.AuthorId, cancellationToken))
            return null;

        var book = new Book
        {
            Title = request.Title,
            Description = request.Description,
            PublishDate = request.PublishDate,
            Isbn = request.Isbn,
            AuthorId = request.AuthorId
        };

        context.Books.Add(book);
        await context.SaveChangesAsync(cancellationToken);
        return book.Id;
    }

    public async Task<bool> Update(Guid id, UpdateBookRequest request, CancellationToken cancellationToken = default)
    {
        if (!formatter.ValidateIsbn13(request.Isbn ?? ""))
            return false;

        if (await context.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken) is not {} book)
            return false;

        if (!string.IsNullOrWhiteSpace(request.Title))
            book.Title = request.Title;

        if (!string.IsNullOrWhiteSpace(request.Description))
            book.Description = request.Description;

        if (request.PublishDate is {} publishDate)
            book.PublishDate = publishDate;

        if (!string.IsNullOrWhiteSpace(request.Isbn))
            book.Isbn = request.Isbn;

        if (request.AuthorId is {} authorId
            && await context.Persons.AnyAsync(x => x.Id == authorId, cancellationToken))
            book.AuthorId = authorId;

        if (!context.ChangeTracker.HasChanges())
            return false;

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        if (await context.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken) is not {} book)
            return false;

        context.Books.Remove(book);

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}