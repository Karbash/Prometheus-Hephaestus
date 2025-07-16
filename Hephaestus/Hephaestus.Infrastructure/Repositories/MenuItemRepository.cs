using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hephaestus.Infrastructure.Repositories;

public class MenuItemRepository : IMenuItemRepository
{
    private readonly HephaestusDbContext _context;

    public MenuItemRepository(HephaestusDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<MenuItem>> GetByTenantIdAsync(string tenantId, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("TenantId é obrigatório.");

        var query = _context.MenuItems
            .Include(m => m.MenuItemTags)
            .ThenInclude(mt => mt.Tag)
            .AsNoTracking()
            .Where(m => m.TenantId == tenantId);

        // Ordenação dinâmica
        if (!string.IsNullOrEmpty(sortBy))
        {
            if (sortOrder?.ToLower() == "desc")
                query = query.OrderByDescending(e => EF.Property<object>(e, sortBy));
            else
                query = query.OrderBy(e => EF.Property<object>(e, sortBy));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<MenuItem>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<MenuItem?> GetByIdAsync(string id, string tenantId)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("Id e TenantId são obrigatórios.");
        return await _context.MenuItems
            .Include(m => m.MenuItemTags)
            .ThenInclude(mt => mt.Tag)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == tenantId);
    }

    public async Task AddAsync(MenuItem menuItem)
    {
        if (menuItem == null)
            throw new ArgumentNullException(nameof(menuItem));
        await _context.MenuItems.AddAsync(menuItem);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MenuItem menuItem)
    {
        if (menuItem == null)
            throw new ArgumentNullException(nameof(menuItem));
        var existing = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == menuItem.Id && m.TenantId == menuItem.TenantId);
        if (existing == null)
            return;
        _context.Entry(existing).CurrentValues.SetValues(menuItem);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id, string tenantId)
    {
        var menuItem = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == id && m.TenantId == tenantId);
        if (menuItem == null)
            return;
        _context.MenuItems.Remove(menuItem);
        await _context.SaveChangesAsync();
    }

    public async Task AddTagsAsync(string menuItemId, IEnumerable<string> tagIds, string tenantId)
    {
        var menuItem = await _context.MenuItems.Include(m => m.MenuItemTags).FirstOrDefaultAsync(m => m.Id == menuItemId && m.TenantId == tenantId);
        if (menuItem == null)
            throw new ArgumentException("MenuItem não encontrado.");
        var tags = await _context.Tags.Where(t => tagIds.Contains(t.Id) && t.TenantId == tenantId).ToListAsync();
        menuItem.MenuItemTags = tags.Select(t => new MenuItemTag { MenuItemId = menuItemId, TagId = t.Id }).ToList();
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ValidateTagIdsAsync(IEnumerable<string> tagIds, string tenantId)
    {
        var count = await _context.Tags.CountAsync(t => tagIds.Contains(t.Id) && t.TenantId == tenantId);
        return count == tagIds.Count();
    }
} 
