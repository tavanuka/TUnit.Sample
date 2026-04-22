namespace TUnit.Sample.Common.Contracts.Books;

public record CreateBookRequest(string Title, string Description, DateTime PublishDate, string Isbn, Guid AuthorId);

public record UpdateBookRequest(string? Title, string? Description, DateTime? PublishDate, string? Isbn, Guid? AuthorId);
