using Hephaestus.Domain.Entities;
using Hephaestus.Domain.DTOs.Response;

namespace Hephaestus.Domain.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(string id, string tenantId);
    Task<Category?> GetByNameAsync(string name, string tenantId);
    Task<PagedResult<Category>> GetByTenantIdAsync(string tenantId, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
    Task<IEnumerable<Category>> GetAllActiveAsync(string tenantId);
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(string id, string tenantId);
    Task<bool> ExistsAsync(string id, string tenantId);
    Task<bool> ExistsByNameAsync(string name, string tenantId);
} 