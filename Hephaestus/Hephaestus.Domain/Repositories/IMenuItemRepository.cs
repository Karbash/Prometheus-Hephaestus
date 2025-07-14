using Hephaestus.Domain.Entities;
using Hephaestus.Application.DTOs.Response;

namespace Hephaestus.Domain.Repositories;

public interface IMenuItemRepository
{
    Task AddAsync(MenuItem menuItem);
    Task UpdateAsync(MenuItem menuItem);
    Task DeleteAsync(string id, string tenantId);
    Task<MenuItem?> GetByIdAsync(string id, string tenantId);
    Task<PagedResult<MenuItem>> GetByTenantIdAsync(string tenantId, int pageNumber = 1, int pageSize = 20);
    Task AddTagsAsync(string menuItemId, IEnumerable<string> tagIds, string tenantId);
    Task<bool> ValidateTagIdsAsync(IEnumerable<string> tagIds, string tenantId);
}