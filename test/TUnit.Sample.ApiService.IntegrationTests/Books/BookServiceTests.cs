using Bogus;
using TUnit.Mocks.Generated.TUnit.Sample.ApiService.Services;
using TUnit.Sample.ApiService.Endpoints.Books;
using TUnit.Sample.ApiService.Services;
using PersonEntity=TUnit.Sample.Domain.Person;

namespace TUnit.Sample.ApiService.IntegrationTests.Books;

public class BookServiceTests : CoreIntegrationTestBase
{
    private const int Seed = 42069;

    // Hint: all private fields that are not static are created new per test per class.
    private static readonly Faker<PersonEntity> PersonGenerator = new Faker<PersonEntity>()
        .UseSeed(Seed)
        .RuleFor(p => p.FirstName, f => f.Name.FirstName())
        .RuleFor(p => p.LastName, f => f.Name.LastName())
        .RuleFor(p => p.BirthDate, f => f.Date.Past(50).ToUniversalTime());
    
    // Source generated interfaces
    private readonly IBookEventPublisherMock _publisher = IBookEventPublisher.Mock();
    private readonly IIsbnFormatterMock _formatter = IIsbnFormatter.Mock();
    private readonly IDateTimeProviderMock _timeProvider = IDateTimeProvider.Mock();
    
    public BookServiceTests()
    {
        Randomizer.Seed = new Random(Seed);
    }

    [Before(Test)]
    public Task OtherBeforeTest()
    {
        try
        {
            Console.WriteLine("Before Test 12");
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }
    
    [Test]
    public async Task Insert_OnValidCreation_ShouldPublishEvent()
    {
        // Arrange
        var sut = new BookService(DbContext, _formatter, _timeProvider, _publisher);
        var person = PersonGenerator.Generate();
        person.Id = Guid.CreateVersion7();

        DbContext.Persons.Add(person);
        await DbContext.SaveChangesAsync();

        // Mocks using FluentAPI
        _timeProvider.UtcNow.Returns(DateTime.UtcNow.AddDays(-42));
        _formatter.ValidateIsbn13(Any<string>()).Returns(true);
        
        var faker = new Faker();
        var createBookRequest = new CreateBookRequest(
            faker.Lorem.Sentence(4),
            faker.Lorem.Paragraph(),
            faker.Date.Between(new DateTime(1900, 1, 1), DateTime.UtcNow)
                .ToUniversalTime(),
            faker.Commerce.Ean13(),
            person.Id
        );

        // Act
        var result = await sut.Insert(createBookRequest);

        // Assert
        await Assert.That(result)
            .IsNotNull()
            .And.IsNotEmptyGuid();

        _publisher.BookCreatedAsync(Is(result!.Value), Any<string>(), Any<CancellationToken>())
            .WasCalled(Times.Once);
    }
}