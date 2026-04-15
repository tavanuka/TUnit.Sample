namespace TUnit.Sample.ApiService.Services;

public class LoggingBookEventPublisher(ILogger<LoggingBookEventPublisher> logger) : IBookEventPublisher
{
    public Task BookCreatedAsync(Guid bookId, string title, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Book created: {BookId} ({Title})", bookId, title);
        return Task.CompletedTask;
    }

    public Task BookUpdatedAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Book updated: {BookId}", bookId);
        return Task.CompletedTask;
    }

    public Task BookDeletedAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Book deleted: {BookId}", bookId);
        return Task.CompletedTask;
    }
}