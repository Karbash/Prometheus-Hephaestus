using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hephaestus.Infrastructure.Repositories;

public class MenuItemRepository : IMenuItemRepository
{
    private readonly HephaestusDbContext _context;

    public MenuItemRepository(HephaestusDbContext context)
    {
        _context = context;
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

        var existingMenuItem = await _context.MenuItems
            .Include(m => m.MenuItemTags)
            .FirstOrDefaultAsync(m => m.Id == menuItem.Id && m.TenantId == menuItem.TenantId);

        if (existingMenuItem == null)
            return; // Não lança exceção, deixa o UseCase tratar

        _context.Entry(existingMenuItem).CurrentValues.SetValues(menuItem);
        existingMenuItem.MenuItemTags = menuItem.MenuItemTags;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id, string tenantId)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("Id e TenantId são obrigatórios.");

        var menuItem = await _context.MenuItems
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == tenantId);

        if (menuItem == null)
            return; // Não lança exceção, deixa o UseCase tratar

        _context.MenuItems.Remove(menuItem);
        await _context.SaveChangesAsync();
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

    public async Task<IEnumerable<MenuItem>> GetByTenantIdAsync(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("TenantId é obrigatório.");

        return await _context.MenuItems
            .Include(m => m.MenuItemTags)
            .ThenInclude(mt => mt.Tag)
            .AsNoTracking()
            .Where(m => m.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task AddTagsAsync(string menuItemId, IEnumerable<string> tagIds, string tenantId)
    {
        if (string.IsNullOrEmpty(menuItemId) || string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("MenuItemId e TenantId são obrigatórios.");

        var menuItem = await _context.MenuItems
            .Include(m => m.MenuItemTags)
            .FirstOrDefaultAsync(m => m.Id == menuItemId && m.TenantId == tenantId);

        if (menuItem == null)
            return; // Não lança exceção, deixa o UseCase tratar

        var validTagIds = await _context.Tags
            .Where(t => t.TenantId == tenantId && tagIds.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync();

        if (validTagIds.Count != tagIds.Count())
            throw new InvalidOperationException("Um ou mais TagIds são inválidos para este tenant.");

        menuItem.MenuItemTags.Clear();
        menuItem.MenuItemTags.AddRange(tagIds.Select(tagId => new MenuItemTag
        {
            MenuItemId = menuItemId,
            TagId = tagId
        }));

        await _context.SaveChangesAsync();
    }

    public async Task<bool> ValidateTagIdsAsync(IEnumerable<string> tagIds, string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("TenantId é obrigatório.");

        var validTagIds = await _context.Tags
            .Where(t => t.TenantId == tenantId && tagIds.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync();

        return validTagIds.Count == tagIds.Count();
    }
}