namespace TUnit.Sample.Domain;

public class Book
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime PublishDate { get; set; }
    public string Isbn { get; set; } = string.Empty;

    public Person Author { get; set; } = null!;
    public Guid AuthorId { get; set; }
}