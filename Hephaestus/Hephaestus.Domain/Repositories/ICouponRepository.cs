using Hephaestus.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hephaestus.Domain.Repositories;

public interface ICouponRepository
{
    Task AddAsync(Coupon coupon);
    Task<Coupon?> GetByIdAsync(string id, string tenantId);
    Task<IEnumerable<Coupon>> GetByTenantIdAsync(string tenantId, bool? isActive, string? customerPhoneNumber);
    Task UpdateAsync(Coupon coupon);
    Task DeleteAsync(string id, string tenantId);
    Task<bool> CodeExistsAsync(string code, string tenantId);
}