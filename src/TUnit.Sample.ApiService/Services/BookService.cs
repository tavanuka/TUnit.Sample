namespace TUnit.Sample.ApiService.Services;

public class BookService
{
    public bool ValidateIsbn13(string isbn)
    {
        var digits = isbn.Where(char.IsDigit).ToArray();
        if (digits.Length != 13)
            return false;

        var sum = 0;
        for (var i = 0; i < 12; i++)
        {
            var digit = digits[i] - '0';
            sum += (i % 2 == 0) ? digit : digit * 3;
        }

        var checkDigit = (10 - (sum % 10)) % 10;
        return checkDigit == (digits[12] - '0');
    }

    public string FormatIsbn(string isbn)
    {
        var digits = new string(isbn.Where(char.IsDigit).ToArray());
        if (digits.Length != 13)
            return isbn;

        // Format: XXX-X-XX-XXXXXX-X (EAN-group-publisher-title-check)
        return $"{digits[..3]}-{digits[3]}-{digits[4..6]}-{digits[6..12]}-{digits[12]}";
    }
}
