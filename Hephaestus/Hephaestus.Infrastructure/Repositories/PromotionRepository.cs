using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hephaestus.Infrastructure.Repositories;

public class PromotionRepository : IPromotionRepository
{
    private readonly HephaestusDbContext _dbContext;

    public PromotionRepository(HephaestusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Promotion promotion)
    {
        await _dbContext.Promotions.AddAsync(promotion);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<PagedResult<Promotion>> GetByTenantIdAsync(string tenantId, bool? isActive = null, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        var query = _dbContext.Promotions
            .Where(p => p.TenantId == tenantId);

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        // Ordenação dinâmica
        if (!string.IsNullOrEmpty(sortBy))
        {
            query = sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                : query.OrderBy(e => EF.Property<object>(e, sortBy));
        }
        else
        {
            query = query.OrderByDescending(p => p.StartDate);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Promotion>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<Promotion?> GetByIdAsync(string id, string tenantId)
    {
        return await _dbContext.Promotions
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);
    }

    public async Task UpdateAsync(Promotion promotion)
    {
        _dbContext.Promotions.Update(promotion);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id, string tenantId)
    {
        var promotion = await GetByIdAsync(id, tenantId);
        if (promotion != null)
        {
            _dbContext.Promotions.Remove(promotion);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task AddUsageAsync(PromotionUsage usage)
    {
        await _dbContext.PromotionUsages.AddAsync(usage);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<int> GetUsageCountAsync(string promotionId, string tenantId)
    {
        return await _dbContext.PromotionUsages.CountAsync(u => u.PromotionId == promotionId && u.TenantId == tenantId);
    }

    public async Task<int> GetUsageCountByCustomerAsync(string promotionId, string tenantId, string customerPhoneNumber)
    {
        return await _dbContext.PromotionUsages.CountAsync(u => u.PromotionId == promotionId && u.TenantId == tenantId && u.CustomerPhoneNumber == customerPhoneNumber);
    }
}
