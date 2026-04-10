using TUnit.Sample.ApiService.Services;

namespace TUnit.Sample.ApiService.Tests.Services;

public class PersonServiceTests
{
    private readonly PersonService _sut = new();

    [Test]
    public async Task CalculateAge_BirthdayAlreadyPassed_ReturnsCorrectAge()
    {
        var birthDate = new DateTime(1990, 3, 15, 0, 0, 0, DateTimeKind.Utc);
        var reference = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var age = _sut.CalculateAge(birthDate, reference);

        await Assert.That(age).IsEqualTo(34);
    }

    [Test]
    public async Task CalculateAge_BirthdayNotYetPassed_ReturnsOneYearLess()
    {
        var birthDate = new DateTime(1990, 8, 20, 0, 0, 0, DateTimeKind.Utc);
        var reference = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var age = _sut.CalculateAge(birthDate, reference);

        await Assert.That(age).IsEqualTo(33);
    }

    [Test]
    public async Task CalculateAge_ExactBirthday_ReturnsCorrectAge()
    {
        var birthDate = new DateTime(1990, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var reference = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var age = _sut.CalculateAge(birthDate, reference);

        await Assert.That(age).IsEqualTo(34);
    }

    [Test]
    public async Task CalculateAge_Newborn_ReturnsZero()
    {
        var birthDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var reference = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var age = _sut.CalculateAge(birthDate, reference);

        await Assert.That(age).IsEqualTo(0);
    }

    [Test]
    public async Task IsMinor_Under18_ReturnsTrue()
    {
        var birthDate = new DateTime(2010, 5, 10, 0, 0, 0, DateTimeKind.Utc);
        var reference = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var result = _sut.IsMinor(birthDate, reference);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsMinor_Exactly18_ReturnsFalse()
    {
        var birthDate = new DateTime(2006, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var reference = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var result = _sut.IsMinor(birthDate, reference);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task IsMinor_Over18_ReturnsFalse()
    {
        var birthDate = new DateTime(1990, 3, 15, 0, 0, 0, DateTimeKind.Utc);
        var reference = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var result = _sut.IsMinor(birthDate, reference);

        await Assert.That(result).IsFalse();
    }
}
