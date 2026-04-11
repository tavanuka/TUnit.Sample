using Microsoft.EntityFrameworkCore;
using TUnit.Sample.ApiService.Endpoints.Persons;
using TUnit.Sample.Domain;
using TUnit.Sample.Infrastructure;

namespace TUnit.Sample.ApiService.Services;

public class PersonService(CoreDbContext context, IAgeCalculator ageCalculator)
{
    public async Task<List<PersonResponse>> GetAll(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var persons = await context.Persons.AsNoTracking()
            .Select(p => new PersonResponse(
                p.Id, p.FirstName, p.LastName, p.BirthDate,
                ageCalculator.CalculateAge(p.BirthDate, now),
                ageCalculator.IsMinor(p.BirthDate, now)))
            .ToListAsync(cancellationToken: cancellationToken);
        return persons;
    }
    
    public async Task<PersonDetailResponse?> GetById(Guid id, CancellationToken ct)
    {
        if (id == Guid.Empty)
            return null;

        var now = DateTime.UtcNow;
        var person = await context.Persons
            .AsNoTracking()
            .Select(p => new PersonDetailResponse(
                p.Id,
                p.FirstName,
                p.LastName,
                p.BirthDate,
                ageCalculator.CalculateAge(p.BirthDate, now),
                ageCalculator.IsMinor(p.BirthDate, now),
                p.AuthoredBooks.Select(b => new PersonBookResponse(b.Id, b.Title, b.Isbn)).ToList()))
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken: ct);

        return person;
    }
    
    public async Task<Guid?> Insert(CreatePersonRequest request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var person = new Person
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = request.BirthDate
            };

            context.Persons.Add(person);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            
            return person.Id;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }
    }
}