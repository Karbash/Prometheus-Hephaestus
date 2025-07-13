using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories;

public interface IPromotionRepository
{
    Task AddAsync(Promotion promotion);
    Task<IEnumerable<Promotion>> GetByTenantIdAsync(string tenantId, bool? isActive = null);
    Task<Promotion> GetByIdAsync(string id, string tenantId);
    Task UpdateAsync(Promotion promotion);
    Task DeleteAsync(string id, string tenantId);
}