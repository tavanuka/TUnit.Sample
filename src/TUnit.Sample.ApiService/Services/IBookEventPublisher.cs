namespace TUnit.Sample.ApiService.Services;

public interface IBookEventPublisher
{
    Task BookCreatedAsync(Guid bookId, string title, CancellationToken cancellationToken = default);
    Task BookUpdatedAsync(Guid bookId, CancellationToken cancellationToken = default);
    Task BookDeletedAsync(Guid bookId, CancellationToken cancellationToken = default);
}