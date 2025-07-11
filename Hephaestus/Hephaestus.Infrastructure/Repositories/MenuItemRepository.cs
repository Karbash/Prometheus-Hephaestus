using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hephaestus.Infrastructure.Repositories;

public class MenuItemRepository : IMenuItemRepository
{
    private readonly HephaestusDbContext _context;
    private readonly ITagRepository _tagRepository;

    public MenuItemRepository(HephaestusDbContext context, ITagRepository tagRepository)
    {
        _context = context;
        _tagRepository = tagRepository;
    }

    public async Task AddAsync(MenuItem menuItem)
    {
        await _context.MenuItems.AddAsync(menuItem);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<MenuItem>> GetByTenantIdAsync(string tenantId)
    {
        return await _context.MenuItems
            .Include(m => m.MenuItemTags)
            .ThenInclude(mt => mt.Tag)
            .Where(m => m.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<MenuItem?> GetByIdAsync(string id, string tenantId)
    {
        return await _context.MenuItems
            .Include(m => m.MenuItemTags)
            .ThenInclude(mt => mt.Tag)
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == tenantId);
    }

    public async Task UpdateAsync(MenuItem menuItem)
    {
        _context.MenuItems.Update(menuItem);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id, string tenantId)
    {
        var menuItem = await GetByIdAsync(id, tenantId);
        if (menuItem != null)
        {
            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new KeyNotFoundException("Item do cardápio não encontrado.");
        }
    }

    public async Task AddTagsAsync(string menuItemId, IEnumerable<string> tagNames, string tenantId)
    {
        var menuItem = await GetByIdAsync(menuItemId, tenantId);
        if (menuItem == null)
            throw new KeyNotFoundException("Item do cardápio não encontrado.");

        foreach (var tagName in tagNames)
        {
            var tag = await _tagRepository.GetByNameAsync(tagName, tenantId);
            if (tag == null)
            {
                tag = new Tag { TenantId = tenantId, Name = tagName };
                await _tagRepository.AddAsync(tag);
            }

            if (!menuItem.MenuItemTags.Any(mt => mt.TagId == tag.Id))
            {
                menuItem.MenuItemTags.Add(new MenuItemTag { MenuItemId = menuItemId, TagId = tag.Id });
            }
        }

        await _context.SaveChangesAsync();
    }
}