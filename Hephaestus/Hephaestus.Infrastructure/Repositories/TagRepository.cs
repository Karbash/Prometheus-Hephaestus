using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hephaestus.Infrastructure.Repositories;

public class TagRepository : ITagRepository
{
    private readonly HephaestusDbContext _context;

    public TagRepository(HephaestusDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Tag>> GetByTenantIdAsync(string tenantId, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("TenantId é obrigatório.");

        var query = _context.Tags.AsNoTracking().Where(t => t.TenantId == tenantId);

        if (!string.IsNullOrEmpty(sortBy))
        {
            if (sortOrder?.ToLower() == "desc")
                query = query.OrderByDescending(e => EF.Property<object>(e, sortBy));
            else
                query = query.OrderBy(e => EF.Property<object>(e, sortBy));
        }

        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<Tag>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<Tag?> GetByIdAsync(string id, string tenantId)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("Id e TenantId são obrigatórios.");
        return await _context.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);
    }

    public async Task<Tag?> GetByNameAsync(string name, string tenantId)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("Name e TenantId são obrigatórios.");
        return await _context.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Name == name && t.TenantId == tenantId);
    }

    public async Task AddAsync(Tag tag)
    {
        if (tag == null)
            throw new ArgumentNullException(nameof(tag));
        await _context.Tags.AddAsync(tag);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Tag tag)
    {
        if (tag == null)
            throw new ArgumentNullException(nameof(tag));
        var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == tag.Id && t.TenantId == tag.TenantId);
        if (existingTag == null)
            return;
        _context.Entry(existingTag).CurrentValues.SetValues(tag);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id, string tenantId)
    {
        var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);
        if (tag == null)
            return;
        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();
    }
} 
