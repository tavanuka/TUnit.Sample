namespace TUnit.Sample.ApiService.Services;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}