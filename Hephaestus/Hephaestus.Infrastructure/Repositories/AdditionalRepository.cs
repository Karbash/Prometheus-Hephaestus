using Hephaestus.Domain.DTOs.Response;
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

    public async Task<PagedResult<Additional>> GetByTenantIdAsync(string tenantId, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        var query = _dbContext.Additionals
            .Where(a => a.TenantId == tenantId);

        // Ordena��o din�mica
        if (!string.IsNullOrEmpty(sortBy))
        {
            query = sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                : query.OrderBy(e => EF.Property<object>(e, sortBy));
        }
        else
        {
            query = query.OrderByDescending(a => a.Name);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Additional>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
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
        if (additional != null)
        {
            _dbContext.Additionals.Remove(additional);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<PagedResult<Additional>> GetAllGlobalAsync(
        string? tenantId = null,
        string? name = null,
        bool? isAvailable = null,
        decimal? precoMin = null,
        decimal? precoMax = null,
        DateTime? dataInicial = null,
        DateTime? dataFinal = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc")
    {
        var query = _dbContext.Additionals.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(tenantId))
            query = query.Where(a => a.TenantId == tenantId);
        if (!string.IsNullOrEmpty(name))
            query = query.Where(a => a.Name.Contains(name));
        if (isAvailable.HasValue)
            query = query.Where(a => a.IsAvailable == isAvailable.Value);
        if (precoMin.HasValue)
            query = query.Where(a => a.Price >= precoMin.Value);
        if (precoMax.HasValue)
            query = query.Where(a => a.Price <= precoMax.Value);
        if (dataInicial.HasValue)
            query = query.Where(a => a.CreatedAt >= dataInicial.Value);
        if (dataFinal.HasValue)
            query = query.Where(a => a.CreatedAt <= dataFinal.Value);

        // Ordenação dinâmica
        if (!string.IsNullOrEmpty(sortBy))
        {
            query = sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                : query.OrderBy(e => EF.Property<object>(e, sortBy));
        }
        else
        {
            query = query.OrderByDescending(a => a.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Additional>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
