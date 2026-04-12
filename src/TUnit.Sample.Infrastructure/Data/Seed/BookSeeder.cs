using Bogus;
using Microsoft.EntityFrameworkCore;
using TUnit.Sample.Domain;
using PersonEntity=TUnit.Sample.Domain.Person;

namespace TUnit.Sample.Infrastructure.Data.Seed;

public static class BookSeeder
{
    public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        var booksSet = context.Set<Book>();
        var peopleSet = context.Set<PersonEntity>();

        if (await booksSet.AnyAsync(cancellationToken))
            return;

        if (!await peopleSet.AnyAsync(cancellationToken))
            throw new NullReferenceException("People must be seeded before books");

        // Bogus overloads are confusing the return type of the hash-set, so generic base type is used
        // to enforce the correct overload method.
        ICollection<Guid> peopleIds = await peopleSet
            .AsNoTracking()
            .Select(x => x.Id)
            .ToHashSetAsync(cancellationToken);

        var bookFaker = new Faker<Book>()
            .UseSeed(SeedConstants.Seed)
            .RuleFor(x => x.Isbn, f => f.Commerce.Ean13())
            .RuleFor(x => x.Title, f => f.Lorem.Sentence(4))
            .RuleFor(x => x.PublishDate, f => f.Date.Past(10).ToUniversalTime())
            .RuleFor(x => x.Description, f => f.Lorem.Paragraph())
            .RuleFor(x => x.AuthorId, f => f.PickRandom(peopleIds));

        var books = bookFaker.Generate(420);
        booksSet.AddRange(books);

        await context.SaveChangesAsync(cancellationToken);
    }
}