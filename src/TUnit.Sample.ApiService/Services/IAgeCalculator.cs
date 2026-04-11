namespace TUnit.Sample.ApiService.Services;

public interface IAgeCalculator
{
    bool IsMinor(DateTime birthDate, DateTime referenceDate);
    int CalculateAge(DateTime birthDate, DateTime referenceDate);
}