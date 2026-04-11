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

        group.MapGet("/", async (PersonService personService, CancellationToken ct) => Results.Ok(await personService.GetAll(ct)));

        group.MapGet("/{id:guid}", async (Guid id, PersonService personService, CancellationToken ct) => {
            var person = await personService.GetById(id, ct);
            return person is null
                ? Results.NotFound()
                : Results.Ok(person);
        });

        group.MapPost("/", async (CreatePersonRequest request, PersonService personService, CancellationToken ct) =>
            await personService.Insert(request, ct) is {} id
                ? Results.Created($"/persons/{id}", (object?)id)
                : Results.BadRequest()
        );

        group.MapPut("/{id:guid}", async (Guid id, UpdatePersonRequest request, CoreDbContext db) => {
            var person = await db.Persons.FindAsync(id);
            if (person is null)
                return Results.NotFound();

            person.FirstName = request.FirstName;
            person.LastName = request.LastName;
            person.BirthDate = request.BirthDate;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapDelete("/{id:guid}", async (Guid id, CoreDbContext db) => {
            var person = await db.Persons.FindAsync(id);
            if (person is null)
                return Results.NotFound();

            db.Persons.Remove(person);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}