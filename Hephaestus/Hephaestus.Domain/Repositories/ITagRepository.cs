using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories;

public interface ITagRepository
{
    Task<Tag?> GetByNameAsync(string name, string tenantId);
    Task<IEnumerable<Tag>> GetByTenantIdAsync(string tenantId);
    Task<Tag?> GetByIdAsync(string id, string tenantId); // Novo método
    Task AddAsync(Tag tag);
    Task DeleteAsync(string id, string tenantId);
}