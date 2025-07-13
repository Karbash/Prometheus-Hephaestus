using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories;

public interface IAdditionalRepository
{
    Task AddAsync(Additional additional);
    Task<IEnumerable<Additional>> GetByTenantIdAsync(string tenantId);
    Task<Additional> GetByIdAsync(string id, string tenantId);
    Task UpdateAsync(Additional additional);
    Task DeleteAsync(string id, string tenantId);
}