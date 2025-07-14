using Hephaestus.Application.DTOs.Response;

namespace Hephaestus.Domain.Repositories
{
    public interface ICompanyRepository
    {
        Task<PagedResult<Company>> GetAllAsync(bool? isEnabled, int pageNumber = 1, int pageSize = 20);
    }
} 