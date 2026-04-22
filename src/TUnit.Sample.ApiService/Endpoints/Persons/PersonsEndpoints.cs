using Microsoft.AspNetCore.Mvc;
using TUnit.Sample.ApiService.Services;
using TUnit.Sample.Common.Contracts.Persons;

namespace TUnit.Sample.ApiService.Endpoints.Persons;

public static class PersonsEndpoints
{
    public static void MapPersonsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/persons");

        group.MapGet("/", async ([FromServices] IPersonService personService, CancellationToken ct)
            => Results.Ok(await personService.GetAll(ct))
        );

        group.MapGet("/{id:guid}", async (Guid id, [FromServices] IPersonService personService, CancellationToken ct) => {
            var person = await personService.GetById(id, ct);
            return person is null
                ? Results.NotFound()
                : Results.Ok(person);
        });

        group.MapPost("/", async ([FromBody] CreatePersonRequest request, [FromServices] IPersonService personService, CancellationToken ct) =>
            await personService.Insert(request, ct) is {} id
                ? Results.Created($"/persons/{id}", id)
                : Results.BadRequest()
        );

        group.MapPatch("/{id:guid}", async (Guid id, [FromBody] UpdatePersonRequest request, [FromServices] IPersonService personService, CancellationToken ct)
            => await personService.Update(request, id, ct)
                ? Results.NoContent()
                : Results.NotFound()
        );

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdatePersonRequest request, [FromServices] IPersonService personService, CancellationToken ct) => {
            var updated = await personService.Update(request, id, ct);
            if (updated)
                return Results.NoContent();

            // TODO: specify FluentValidation rules instead 
            if (!request.BirthDate.HasValue
                || string.IsNullOrWhiteSpace(request.FirstName)
                || string.IsNullOrWhiteSpace(request.LastName))
                return Results.BadRequest("Person could not be found or invalid data provided.");

            var createRequest = new CreatePersonRequest(request.FirstName, request.LastName, request.BirthDate.Value);

            return await personService.Insert(createRequest, ct) is {} createdId
                ? Results.Created($"/persons/{createdId}", createdId)
                : Results.BadRequest();
        });

        group.MapDelete("/{id:guid}", async (Guid id, [FromServices] IPersonService personService, CancellationToken ct)
            => await personService.Delete(id, ct)
                ? Results.NoContent()
                : Results.NotFound()
        );
    }
}