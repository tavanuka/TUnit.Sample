using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TUnit.Sample.ApiService.Services;
using TUnit.Sample.Infrastructure;

namespace TUnit.Sample.ApiService.Endpoints.Persons;

public record PersonResponse(Guid Id, string FirstName, string LastName, DateTime BirthDate, int Age, bool IsMinor);

public record PersonDetailResponse(Guid Id, string FirstName, string LastName, DateTime BirthDate, int Age, bool IsMinor, List<PersonBookResponse> AuthoredBooks);

public record PersonBookResponse(Guid Id, string Title, string Isbn);

public record CreatePersonRequest(string FirstName, string LastName, DateTime BirthDate);

public record UpdatePersonRequest(string? FirstName, string? LastName, DateTime? BirthDate);

public static class PersonsEndpoints
{
    public static void MapPersonsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/persons");

        group.MapGet("/", async (IPersonService personService, CancellationToken ct)
            => Results.Ok(await personService.GetAll(ct))
        );

        group.MapGet("/{id:guid}", async (Guid id, PersonService personService, CancellationToken ct) => {
            var person = await personService.GetById(id, ct);
            return person is null
                ? Results.NotFound()
                : Results.Ok(person);
        });

        group.MapPost("/", async (CreatePersonRequest request, IPersonService personService, CancellationToken ct) =>
            await personService.Insert(request, ct) is {} id
                ? Results.Created($"/persons/{id}", (object?)id)
                : Results.BadRequest()
        );

        group.MapPatch("/{id:guid}", async (Guid id, [FromBody] UpdatePersonRequest request, IPersonService personService, CancellationToken ct)
            => await personService.Update(request, id, ct)
                ? Results.NoContent()
                : Results.NotFound()
        );

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdatePersonRequest request, IPersonService personService, CancellationToken ct) => {
            var updated = await personService.Update(request, id, ct);
            if (updated)
                return Results.NoContent();

            if (!request.BirthDate.HasValue
                || string.IsNullOrWhiteSpace(request.FirstName)
                || string.IsNullOrWhiteSpace(request.LastName))
                return Results.BadRequest("Person could not be found or invalid data provided.");

            var createRequest = new CreatePersonRequest(request.FirstName, request.LastName, request.BirthDate.Value);
            await personService.Insert(createRequest, ct);

            return Results.Created($"/persons/{id}", id);
        });

        group.MapDelete("/{id:guid}", async (Guid id, IPersonService personService, CancellationToken ct)
            => await personService.Delete(id, ct)
                ? Results.NoContent()
                : Results.NotFound()
        );
    }
}