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

    public async Task<IEnumerable<Promotion>> GetByTenantIdAsync(string tenantId, bool? isActive = null)
    {
        var query = _dbContext.Promotions
            .Where(p => p.TenantId == tenantId);

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        return await query.ToListAsync();
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
        _dbContext.Promotions.Remove(promotion);
        await _dbContext.SaveChangesAsync();
    }
}