using Hephaestus.Application.DTOs.Response;
using Microsoft.EntityFrameworkCore;

namespace Hephaestus.Infrastructure.Repositories;

public class MenuItemRepository : IMenuItemRepository
{
    private readonly ApplicationDbContext _context;

    public MenuItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<MenuItem>> GetByTenantIdAsync(string tenantId, int pageNumber = 1, int pageSize = 20)
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("TenantId é obrigatório.");

        var query = _context.MenuItems
            .Include(m => m.MenuItemTags)
            .ThenInclude(mt => mt.Tag)
            .AsNoTracking()
            .Where(m => m.TenantId == tenantId);

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
} 