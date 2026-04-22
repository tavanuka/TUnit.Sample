using Refit;
using TUnit.Sample.Common.Contracts.Persons;

namespace TUnit.Sample.Web;

public interface IPersonApi
{
    [Get("/persons")]
    Task<List<PersonResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    [Get("/persons/{id}")]
    Task<IApiResponse<PersonDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
