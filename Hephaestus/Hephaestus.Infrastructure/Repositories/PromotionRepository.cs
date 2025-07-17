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

    public async Task<PagedResult<Promotion>> GetByCompanyIdAsync(string companyId, bool? isActive = null, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        var query = _dbContext.Promotions.AsNoTracking().Where(p => p.CompanyId == companyId);

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

    public async Task<Promotion?> GetByIdAsync(string id, string companyId)
    {
        return await _dbContext.Promotions.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);
    }

    public async Task UpdateAsync(Promotion promotion)
    {
        _dbContext.Promotions.Update(promotion);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id, string companyId)
    {
        var promotion = await GetByIdAsync(id, companyId);
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

    public async Task<int> GetUsageCountAsync(string promotionId, string companyId)
    {
        return await _dbContext.PromotionUsages.CountAsync(u => u.PromotionId == promotionId && u.CompanyId == companyId);
    }

    public async Task<int> GetUsageCountByCustomerAsync(string promotionId, string companyId, string customerPhoneNumber)
    {
        return await _dbContext.PromotionUsages.CountAsync(u => u.PromotionId == promotionId && u.CompanyId == companyId && u.CustomerId == customerPhoneNumber);
    }

    public async Task<PagedResult<Promotion>> GetAllAsync(bool? isActive = null, string? companyId = null, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        var query = _dbContext.Promotions.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(companyId))
            query = query.Where(p => p.CompanyId == companyId);
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

    public Task<PagedResult<Promotion>> GetAllGlobalAsync(string? code = null, bool? isActive = null, string? companyId = null, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        throw new NotImplementedException();
    }
}
