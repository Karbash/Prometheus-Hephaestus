using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hephaestus.Domain.DTOs.Response;

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

    public async Task<PagedResult<Coupon>> GetByTenantIdAsync(string tenantId, bool? isActive = null, string? customerPhoneNumber = null, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("TenantId é obrigatório.");

        var query = _context.Coupons.AsNoTracking().Where(c => c.TenantId == tenantId);
        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);
        if (!string.IsNullOrEmpty(customerPhoneNumber))
            query = query.Where(c => c.CustomerId == customerPhoneNumber);

        if (!string.IsNullOrEmpty(sortBy))
        {
            if (sortOrder?.ToLower() == "desc")
                query = query.OrderByDescending(e => EF.Property<object>(e, sortBy));
            else
                query = query.OrderBy(e => EF.Property<object>(e, sortBy));
        }

        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<Coupon>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
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

    public async Task AddUsageAsync(CouponUsage usage)
    {
        await _context.CouponUsages.AddAsync(usage);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetUsageCountAsync(string couponId, string tenantId)
    {
        return await _context.CouponUsages.CountAsync(u => u.CouponId == couponId && u.TenantId == tenantId);
    }

    public async Task<int> GetUsageCountByCustomerAsync(string couponId, string tenantId, string customerPhoneNumber)
    {
        return await _context.CouponUsages.CountAsync(u => u.CouponId == couponId && u.TenantId == tenantId && u.CustomerId == customerPhoneNumber);
    }
}
