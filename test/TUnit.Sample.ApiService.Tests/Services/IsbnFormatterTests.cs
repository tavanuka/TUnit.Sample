using TUnit.Sample.ApiService.Services;

namespace TUnit.Sample.ApiService.Tests.Services;

public class IsbnFormatterTests
{
    private readonly IsbnFormatter _sut = new();

    [Test]
    public async Task ValidateIsbn13_ValidIsbn_ReturnsTrue()
    {
        // 978-0-306-40615-7 is a well-known valid ISBN-13
        var result = _sut.ValidateIsbn13("9780306406157");

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task ValidateIsbn13_ValidIsbnWithHyphens_ReturnsTrue()
    {
        var result = _sut.ValidateIsbn13("978-0-306-40615-7");

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task ValidateIsbn13_InvalidCheckDigit_ReturnsFalse()
    {
        // Changed last digit from 7 to 8
        var result = _sut.ValidateIsbn13("9780306406158");

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task ValidateIsbn13_TooShort_ReturnsFalse()
    {
        var result = _sut.ValidateIsbn13("978030640");

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task ValidateIsbn13_TooLong_ReturnsFalse()
    {
        var result = _sut.ValidateIsbn13("97803064061571");

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task ValidateIsbn13_AnotherValidIsbn_ReturnsTrue()
    {
        // 978-3-16-148410-0
        var result = _sut.ValidateIsbn13("9783161484100");

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task FormatIsbn_ValidDigits_ReturnsHyphenated()
    {
        var result = _sut.FormatIsbn("9780306406157");

        await Assert.That(result).IsEqualTo("978-0-30-640615-7");
    }

    [Test]
    public async Task FormatIsbn_AlreadyHyphenated_FormatsCorrectly()
    {
        var result = _sut.FormatIsbn("978-0-306-40615-7");

        await Assert.That(result).IsEqualTo("978-0-30-640615-7");
    }

    [Test]
    public async Task FormatIsbn_TooShort_ReturnsOriginal()
    {
        var result = _sut.FormatIsbn("12345");

        await Assert.That(result).IsEqualTo("12345");
    }
}
