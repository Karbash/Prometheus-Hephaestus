using Hephaestus.Domain.Entities;
using Hephaestus.Application.DTOs.Response;

namespace Hephaestus.Domain.Repositories;

public interface ITagRepository
{
    Task<Tag?> GetByNameAsync(string name, string tenantId);
    Task<PagedResult<Tag>> GetByTenantIdAsync(string tenantId, int pageNumber = 1, int pageSize = 20);
    Task<Tag?> GetByIdAsync(string id, string tenantId); // Novo método
    Task AddAsync(Tag tag);
    Task DeleteAsync(string id, string tenantId);
}