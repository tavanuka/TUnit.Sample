namespace TUnit.Sample.Common.Contracts.Persons;

public record PersonResponse(Guid Id, string FirstName, string LastName, DateTime BirthDate, int Age, bool IsMinor);

public record PersonDetailResponse(Guid Id, string FirstName, string LastName, DateTime BirthDate, int Age, bool IsMinor, List<PersonBookResponse> AuthoredBooks);

public record PersonBookResponse(Guid Id, string Title, string Isbn);
