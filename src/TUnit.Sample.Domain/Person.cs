namespace TUnit.Sample.Domain;

public class Person
{
    public Guid Id { get; set; }
    
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }

    public ICollection<Book> AuthoredBooks { get; set; } = [];
    
    public string FullName => $"{FirstName} {LastName}";
}