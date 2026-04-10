using Microsoft.EntityFrameworkCore;
using TUnit.Sample.ApiService.Services;
using TUnit.Sample.Infrastructure;

namespace TUnit.Sample.ApiService.Endpoints.Persons;

public record PersonResponse(Guid Id, string FirstName, string LastName, DateTime BirthDate, int Age, bool IsMinor);
public record PersonDetailResponse(Guid Id, string FirstName, string LastName, DateTime BirthDate, int Age, bool IsMinor, List<PersonBookResponse> AuthoredBooks);
public record PersonBookResponse(Guid Id, string Title, string Isbn);
public record CreatePersonRequest(string FirstName, string LastName, DateTime BirthDate);
public record UpdatePersonRequest(string FirstName, string LastName, DateTime BirthDate);

public static class PersonsEndpoints
{
    public static void MapPersonsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/persons");

        group.MapGet("/", async (CoreDbContext db, PersonService personService) =>
        {
            var now = DateTime.UtcNow;
            var persons = await db.Persons.AsNoTracking().ToListAsync();
            return Results.Ok(persons.Select(p => new PersonResponse(
                p.Id, p.FirstName, p.LastName, p.BirthDate,
                personService.CalculateAge(p.BirthDate, now),
                personService.IsMinor(p.BirthDate, now)
            )));
        });

        group.MapGet("/{id:guid}", async (Guid id, CoreDbContext db, PersonService personService) =>
        {
            var person = await db.Persons
                .AsNoTracking()
                .Include(p => p.AuthoredBooks)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (person is null)
                return Results.NotFound();

            var now = DateTime.UtcNow;
            return Results.Ok(new PersonDetailResponse(
                person.Id, person.FirstName, person.LastName, person.BirthDate,
                personService.CalculateAge(person.BirthDate, now),
                personService.IsMinor(person.BirthDate, now),
                person.AuthoredBooks.Select(b => new PersonBookResponse(b.Id, b.Title, b.Isbn)).ToList()
            ));
        });

        group.MapPost("/", async (CreatePersonRequest request, CoreDbContext db) =>
        {
            var person = new Domain.Person
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = request.BirthDate
            };

            db.Persons.Add(person);
            await db.SaveChangesAsync();

            return Results.Created($"/persons/{person.Id}", new { person.Id });
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdatePersonRequest request, CoreDbContext db) =>
        {
            var person = await db.Persons.FindAsync(id);
            if (person is null)
                return Results.NotFound();

            person.FirstName = request.FirstName;
            person.LastName = request.LastName;
            person.BirthDate = request.BirthDate;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapDelete("/{id:guid}", async (Guid id, CoreDbContext db) =>
        {
            var person = await db.Persons.FindAsync(id);
            if (person is null)
                return Results.NotFound();

            db.Persons.Remove(person);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
