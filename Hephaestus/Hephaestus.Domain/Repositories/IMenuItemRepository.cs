using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories
{
    public interface IMenuItemRepository
    {
        Task<PagedResult<MenuItem>> GetByTenantIdAsync(string tenantId, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
        Task<MenuItem?> GetByIdAsync(string id, string tenantId);
        Task AddAsync(MenuItem menuItem);
        Task UpdateAsync(MenuItem menuItem);
        Task DeleteAsync(string id, string tenantId);
        Task AddTagsAsync(string menuItemId, IEnumerable<string> tagIds, string tenantId);
        Task<bool> ValidateTagIdsAsync(IEnumerable<string> tagIds, string tenantId);
    }
} 