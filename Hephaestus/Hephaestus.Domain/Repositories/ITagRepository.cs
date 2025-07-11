using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories;

public interface ITagRepository
{
    Task AddAsync(Tag tag);
    Task<Tag?> GetByNameAsync(string name, string tenantId);
    Task<IEnumerable<Tag>> GetByTenantIdAsync(string tenantId);
}