namespace TUnit.Sample.ApiService.Services;

public interface IIsbnFormatter
{
    bool ValidateIsbn13(string isbn);
    string FormatIsbn(string isbn);
}