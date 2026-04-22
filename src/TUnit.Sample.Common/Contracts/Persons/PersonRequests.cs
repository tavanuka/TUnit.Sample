namespace TUnit.Sample.Common.Contracts.Persons;

public record CreatePersonRequest(string FirstName, string LastName, DateTime BirthDate);

public record UpdatePersonRequest(string? FirstName, string? LastName, DateTime? BirthDate);
