using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hephaestus.Infrastructure.Repositories;

public class CouponRepository : ICouponRepository
{
    private readonly HephaestusDbContext _context;

    public CouponRepository(HephaestusDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Coupon coupon)
    {
        await _context.Coupons.AddAsync(coupon);
        await _context.SaveChangesAsync();
    }

    public async Task<Coupon?> GetByIdAsync(string id, string tenantId)
    {
        return await _context.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
    }

    public async Task<IEnumerable<Coupon>> GetByTenantIdAsync(string tenantId, bool? isActive, string? customerPhoneNumber)
    {
        var query = _context.Coupons
            .AsNoTracking()
            .Where(c => c.TenantId == tenantId);

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        if (!string.IsNullOrEmpty(customerPhoneNumber))
        {
            query = query.Where(c => c.CustomerPhoneNumber == customerPhoneNumber);
        }

        return await query.ToListAsync();
    }

    public async Task UpdateAsync(Coupon coupon)
    {
        _context.Coupons.Update(coupon);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id, string tenantId)
    {
        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);

        if (coupon != null)
        {
            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> CodeExistsAsync(string code, string tenantId)
    {
        return await _context.Coupons
            .AsNoTracking()
            .AnyAsync(c => c.Code == code && c.TenantId == tenantId);
    }
}