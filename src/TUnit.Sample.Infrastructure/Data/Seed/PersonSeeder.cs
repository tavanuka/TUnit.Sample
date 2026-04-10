using Bogus;
using Microsoft.EntityFrameworkCore;
using PersonEntity=TUnit.Sample.Domain.Person;

namespace TUnit.Sample.Infrastructure.Data.Seed;

public static class PersonSeeder
{
    private static readonly Faker<PersonEntity> PersonFaker = GetPersonFaker();
    
    private static Faker<PersonEntity> GetPersonFaker() => new Faker<PersonEntity>()
        .UseSeed(SeedConstants.Seed)
        .RuleFor(x => x.FirstName, f => f.Person.FirstName)
        .RuleFor(x => x.LastName, f => f.Person.LastName)
        .RuleFor(x => x.BirthDate, f => f.Person.DateOfBirth.ToUniversalTime());


    public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        var peopleSet = context.Set<PersonEntity>();
        if (await peopleSet.AnyAsync(cancellationToken))
            return;
        
        var people = PersonFaker.Generate(69);
        peopleSet.AddRange(people);
        
        await context.SaveChangesAsync(cancellationToken);
    }
}