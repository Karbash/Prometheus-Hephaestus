using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Hephaestus.Domain.DTOs.Response;

namespace Hephaestus.Infrastructure.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly HephaestusDbContext _context;

    public AddressRepository(HephaestusDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Address address)
    {
        await _context.Addresses.AddAsync(address);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Address address)
    {
        _context.Addresses.Update(address);
        await _context.SaveChangesAsync();
    }

    public async Task<Address?> GetByIdAsync(string id)
    {
        return await _context.Addresses.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Address>> GetByEntityAsync(string entityId, string entityType)
    {
        return await _context.Addresses.AsNoTracking()
            .Where(a => a.EntityId == entityId && a.EntityType == entityType)
            .ToListAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id);
        if (address != null)
        {
            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<PagedResult<Address>> GetAllGlobalAsync(
        string? entityId = null,
        string? entityType = null,
        string? city = null,
        string? state = null,
        string? type = null,
        DateTime? dataInicial = null,
        DateTime? dataFinal = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc")
    {
        var query = _context.Addresses.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(entityId))
            query = query.Where(a => a.EntityId == entityId);
        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(a => a.EntityType == entityType);
        if (!string.IsNullOrEmpty(city))
            query = query.Where(a => a.City == city);
        if (!string.IsNullOrEmpty(state))
            query = query.Where(a => a.State == state);
        if (!string.IsNullOrEmpty(type))
            query = query.Where(a => a.Type == type);
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

        return new PagedResult<Address>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
} 