namespace TUnit.Sample.ApiService.Services;

public sealed class AgeCalculator : IAgeCalculator
{
    public bool IsMinor(DateTime birthDate, DateTime referenceDate)
        => CalculateAge(birthDate, referenceDate) < 18;

    public int CalculateAge(DateTime birthDate, DateTime referenceDate)
    {
        var age = referenceDate.Year - birthDate.Year;
        if (referenceDate.Date < birthDate.Date.AddYears(age))
            age--;
        return age;
    }
}