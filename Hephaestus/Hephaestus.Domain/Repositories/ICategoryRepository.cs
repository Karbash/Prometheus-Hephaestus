using Hephaestus.Domain.Entities;
using Hephaestus.Domain.DTOs.Response;

namespace Hephaestus.Domain.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(string id, string tenantId);
    Task<Category?> GetByNameAsync(string name, string? tenantId = null); // Método único com parâmetro opcional
    Task<PagedResult<Category>> GetByTenantIdAsync(string tenantId, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
    Task<IEnumerable<Category>> GetAllActiveAsync(string tenantId);
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(string id, string tenantId);
    Task<bool> ExistsAsync(string id, string tenantId);
    Task<bool> ExistsByNameAsync(string name, string tenantId);
    Task<PagedResult<Category>> GetAllGlobalAsync(string? name = null, string? companyId = null, bool? isActive = null, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
    
    // Novos métodos para suporte híbrido
    Task<PagedResult<Category>> GetHybridCategoriesAsync(string tenantId, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
} 
