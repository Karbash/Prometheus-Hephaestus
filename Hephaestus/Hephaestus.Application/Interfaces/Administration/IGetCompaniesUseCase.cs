using Hephaestus.Domain.DTOs.Response;

namespace Hephaestus.Application.Interfaces.Administration
{
    public interface IGetCompaniesUseCase
    {
        Task<PagedResult<CompanyResponse>> ExecuteAsync(bool? isEnabled, int pageNumber = 1, int pageSize = 20);
    }
} 