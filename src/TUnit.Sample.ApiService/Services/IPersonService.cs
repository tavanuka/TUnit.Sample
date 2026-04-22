using TUnit.Sample.Common.Contracts.Persons;

namespace TUnit.Sample.ApiService.Services;

public interface IPersonService
{
    Task<List<PersonResponse>> GetAll(CancellationToken cancellationToken = default);
    Task<PersonDetailResponse?> GetById(Guid id, CancellationToken ct);
    Task<Guid?> Insert(CreatePersonRequest request, CancellationToken cancellationToken = default);
    Task<bool> Update(UpdatePersonRequest request, Guid id, CancellationToken ct = default);
    Task<bool> Delete(Guid id, CancellationToken ct = default);
}