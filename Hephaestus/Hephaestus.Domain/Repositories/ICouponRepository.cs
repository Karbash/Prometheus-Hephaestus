using Hephaestus.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hephaestus.Domain.DTOs.Response;

namespace Hephaestus.Domain.Repositories;

public interface ICouponRepository
{
    Task AddAsync(Coupon coupon);
    Task<Coupon?> GetByIdAsync(string id, string tenantId);
    Task<PagedResult<Coupon>> GetByTenantIdAsync(string tenantId, bool? isActive = null, string? customerPhoneNumber = null, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
    Task UpdateAsync(Coupon coupon);
    Task DeleteAsync(string id, string tenantId);
    Task<bool> CodeExistsAsync(string code, string tenantId);
    Task AddUsageAsync(CouponUsage usage);
    Task<int> GetUsageCountAsync(string couponId, string tenantId);
    Task<int> GetUsageCountByCustomerAsync(string couponId, string tenantId, string customerPhoneNumber);
    Task<PagedResult<Coupon>> GetAllGlobalAsync(string? code = null, string? companyId = null, string? customerId = null, bool? isActive = null, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
}
