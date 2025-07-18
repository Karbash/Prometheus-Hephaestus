using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories
{
    public interface ITagRepository
    {
        Task<PagedResult<Tag>> GetByCompanyIdAsync(string companyId, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
        Task<Tag?> GetByIdAsync(string id, string companyId);
        Task<Tag?> GetByNameAsync(string name, string? companyId = null); // Método único com parâmetro opcional
        Task AddAsync(Tag tag);
        Task UpdateAsync(Tag tag);
        Task DeleteAsync(string id, string companyId);
        Task<PagedResult<Tag>> GetAllGlobalAsync(string? name = null, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
        
        // Novos métodos para suporte híbrido
        Task<PagedResult<Tag>> GetHybridTagsAsync(string companyId, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
    }
} 
