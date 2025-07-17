using Hephaestus.Domain.Entities;
using Hephaestus.Domain.DTOs.Response;

namespace Hephaestus.Domain.Repositories;

public interface IPromotionRepository
{
    Task AddAsync(Promotion promotion);
    Task<PagedResult<Promotion>> GetByCompanyIdAsync(string companyId, bool? isActive = null, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
    Task<Promotion?> GetByIdAsync(string id, string companyId);
    Task UpdateAsync(Promotion promotion);
    Task DeleteAsync(string id, string companyId);
    Task AddUsageAsync(PromotionUsage usage);
    Task<int> GetUsageCountAsync(string promotionId, string companyId);
    Task<int> GetUsageCountByCustomerAsync(string promotionId, string companyId, string customerPhoneNumber);
    Task<PagedResult<Promotion>> GetAllGlobalAsync(string? code = null, bool? isActive = null, string? companyId = null, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
}
