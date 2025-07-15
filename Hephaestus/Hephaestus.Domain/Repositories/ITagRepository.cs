using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories
{
    public interface ITagRepository
    {
        Task<PagedResult<Tag>> GetByTenantIdAsync(string tenantId, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
        Task<Tag?> GetByIdAsync(string id, string tenantId);
        Task<Tag?> GetByNameAsync(string name, string tenantId);
        Task AddAsync(Tag tag);
        Task UpdateAsync(Tag tag);
        Task DeleteAsync(string id, string tenantId);
    }
} 