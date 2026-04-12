using TUnit.Sample.ApiService.Endpoints.Books;

namespace TUnit.Sample.ApiService.Services;

public interface IBookService
{
    Task<List<BookResponse>> GetAll(CancellationToken cancellationToken = default);
    Task<BookDetailResponse?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<Guid?> Insert(CreateBookRequest request, CancellationToken cancellationToken = default);
    Task<bool> Update(Guid id, UpdateBookRequest request, CancellationToken cancellationToken = default);
    Task<bool> Delete(Guid id, CancellationToken cancellationToken = default);
}