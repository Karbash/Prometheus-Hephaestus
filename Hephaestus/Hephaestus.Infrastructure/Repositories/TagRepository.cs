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

    public async Task<PagedResult<Tag>> GetByCompanyIdAsync(string companyId, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        if (string.IsNullOrEmpty(companyId))
            throw new ArgumentException("CompanyId é obrigatório.");

        var query = _context.Tags.AsNoTracking().Where(t => t.CompanyId == companyId);

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

    public async Task<PagedResult<Tag>> GetAllAsync(
        string? companyId = null,
        string? name = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc")
    {
        var query = _context.Tags.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(companyId))
            query = query.Where(t => t.CompanyId == companyId);
        if (!string.IsNullOrEmpty(name))
            query = query.Where(t => t.Name.Contains(name));

        if (!string.IsNullOrEmpty(sortBy))
        {
            if (sortOrder?.ToLower() == "desc")
                query = query.OrderByDescending(e => EF.Property<object>(e, sortBy));
            else
                query = query.OrderBy(e => EF.Property<object>(e, sortBy));
        }
        else
        {
            query = query.OrderByDescending(t => t.CreatedAt);
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

    public async Task<Tag?> GetByIdAsync(string id, string companyId)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(companyId))
            throw new ArgumentException("Id e CompanyId são obrigatórios.");
        return await _context.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id && t.CompanyId == companyId);
    }

    public async Task<Tag?> GetByNameAsync(string name, string? companyId = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name é obrigatório.");
        
        if (companyId == null)
        {
            // Busca tag global
            return await _context.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Name == name && t.IsGlobal);
        }
        else
        {
            // Busca tag local
            return await _context.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Name == name && t.CompanyId == companyId);
        }
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
        var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == tag.Id && t.CompanyId == tag.CompanyId);
        if (existingTag == null)
            return;
        _context.Entry(existingTag).CurrentValues.SetValues(tag);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id, string companyId)
    {
        var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id && t.CompanyId == companyId);
        if (tag == null)
            return;
        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResult<Tag>> GetAllGlobalAsync(string? name = null, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        var query = _context.Tags.AsNoTracking().Where(t => t.IsGlobal);

        if (!string.IsNullOrEmpty(name))
            query = query.Where(t => t.Name.Contains(name));

        if (!string.IsNullOrEmpty(sortBy))
        {
            if (sortOrder?.ToLower() == "desc")
                query = query.OrderByDescending(e => EF.Property<object>(e, sortBy));
            else
                query = query.OrderBy(e => EF.Property<object>(e, sortBy));
        }
        else
        {
            query = query.OrderByDescending(t => t.CreatedAt);
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

    public async Task<PagedResult<Tag>> GetHybridTagsAsync(string companyId, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        if (string.IsNullOrEmpty(companyId))
            throw new ArgumentException("CompanyId é obrigatório.");

        // Busca tags locais da empresa + tags globais
        var query = _context.Tags.AsNoTracking().Where(t => t.CompanyId == companyId || t.IsGlobal);

        if (!string.IsNullOrEmpty(sortBy))
        {
            if (sortOrder?.ToLower() == "desc")
                query = query.OrderByDescending(e => EF.Property<object>(e, sortBy));
            else
                query = query.OrderBy(e => EF.Property<object>(e, sortBy));
        }
        else
        {
            // Ordena por tipo (globais primeiro) e depois por nome
            query = query.OrderBy(t => !t.IsGlobal).ThenBy(t => t.Name);
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
} 
