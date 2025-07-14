using Hephaestus.Application.DTOs.Response;

namespace Hephaestus.Domain.Repositories
{
    public interface ITagRepository
    {
        Task<PagedResult<Tag>> GetByTenantIdAsync(string tenantId, int pageNumber = 1, int pageSize = 20);
    }
} 