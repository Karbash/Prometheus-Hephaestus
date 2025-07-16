using Hephaestus.Domain.Entities;
using Hephaestus.Domain.DTOs.Response;

namespace Hephaestus.Domain.Repositories;

public interface IPromotionRepository
{
    Task AddAsync(Promotion promotion);
    Task<PagedResult<Promotion>> GetByTenantIdAsync(string tenantId, bool? isActive = null, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
    Task<Promotion?> GetByIdAsync(string id, string tenantId);
    Task UpdateAsync(Promotion promotion);
    Task DeleteAsync(string id, string tenantId);
    Task AddUsageAsync(PromotionUsage usage);
    Task<int> GetUsageCountAsync(string promotionId, string tenantId);
    Task<int> GetUsageCountByCustomerAsync(string promotionId, string tenantId, string customerPhoneNumber);
}
