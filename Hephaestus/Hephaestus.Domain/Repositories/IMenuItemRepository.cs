using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories;

public interface IMenuItemRepository
{
    Task AddAsync(MenuItem menuItem);
    Task UpdateAsync(MenuItem menuItem);
    Task DeleteAsync(string id, string tenantId);
    Task<MenuItem?> GetByIdAsync(string id, string tenantId);
    Task<IEnumerable<MenuItem>> GetByTenantIdAsync(string tenantId);
    Task AddTagsAsync(string menuItemId, IEnumerable<string> tagIds, string tenantId);
    Task<bool> ValidateTagIdsAsync(IEnumerable<string> tagIds, string tenantId);
}