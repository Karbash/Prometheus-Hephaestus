using Hephaestus.Application.DTOs.Response;

namespace Hephaestus.Domain.Repositories
{
    public interface IMenuItemRepository
    {
        Task<PagedResult<MenuItem>> GetByTenantIdAsync(string tenantId, int pageNumber = 1, int pageSize = 20);
    }
} 