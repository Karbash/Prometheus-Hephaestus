using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories;

public interface IMenuItemRepository
{
    Task AddAsync(MenuItem menuItem);
    Task<IEnumerable<MenuItem>> GetByTenantIdAsync(string tenantId);
    Task<MenuItem?> GetByIdAsync(string id, string tenantId);
    Task UpdateAsync(MenuItem menuItem);
    Task DeleteAsync(string id, string tenantId);
    Task AddTagsAsync(string menuItemId, IEnumerable<string> tagNames, string tenantId);
}