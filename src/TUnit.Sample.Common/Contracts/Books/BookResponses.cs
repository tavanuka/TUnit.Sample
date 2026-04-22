namespace TUnit.Sample.Common.Contracts.Books;

public record BookResponse(Guid Id, string Title, string Description, DateTime PublishDate, string Isbn, string FormattedIsbn, Guid AuthorId, string AuthorName);

public record BookDetailResponse(Guid Id, string Title, string Description, DateTime PublishDate, string Isbn, string FormattedIsbn, Guid AuthorId, string AuthorName);
