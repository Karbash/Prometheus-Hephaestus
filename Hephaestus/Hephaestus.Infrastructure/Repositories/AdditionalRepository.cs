using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hephaestus.Infrastructure.Repositories;

public class AdditionalRepository : IAdditionalRepository
{
    private readonly HephaestusDbContext _dbContext;

    public AdditionalRepository(HephaestusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Additional additional)
    {
        await _dbContext.Additionals.AddAsync(additional);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Additional>> GetByTenantIdAsync(string tenantId)
    {
        return await _dbContext.Additionals
            .Where(a => a.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<Additional?> GetByIdAsync(string id, string tenantId)
    {
        return await _dbContext.Additionals
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId);
    }

    public async Task UpdateAsync(Additional additional)
    {
        _dbContext.Additionals.Update(additional);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id, string tenantId)
    {
        var additional = await GetByIdAsync(id, tenantId);
        _dbContext.Additionals.Remove(additional);
        await _dbContext.SaveChangesAsync();
    }
}