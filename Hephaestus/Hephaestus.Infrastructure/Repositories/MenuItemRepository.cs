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

    public async Task<PagedResult<MenuItem>> GetByCompanyIdAsync(string companyId, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc", List<string>? tagIds = null, List<string>? categoryIds = null, decimal? maxPrice = null, bool? promotionActiveNow = null, int? promotionDayOfWeek = null, string? promotionTime = null)
    {
        if (string.IsNullOrEmpty(companyId))
            throw new ArgumentException("CompanyId é obrigatório.");

        var query = _context.MenuItems
            .Include(m => m.MenuItemTags)
            .ThenInclude(mt => mt.Tag)
            .AsNoTracking()
            .Where(m => m.CompanyId == companyId);

        // Filtro por tags (tipo de comida)
        if (tagIds != null && tagIds.Any())
        {
            query = query.Where(m => m.MenuItemTags.Any(mt => tagIds.Contains(mt.TagId)));
        }

        // Filtro por categorias
        if (categoryIds != null && categoryIds.Any())
        {
            query = query.Where(m => categoryIds.Contains(m.CategoryId));
        }

        // Filtro por preço máximo
        if (maxPrice.HasValue)
        {
            query = query.Where(m => m.Price <= maxPrice.Value);
        }

        // Filtro por promoções ativas
        if (promotionActiveNow == true || promotionDayOfWeek.HasValue || !string.IsNullOrWhiteSpace(promotionTime))
        {
            var now = DateTime.UtcNow;
            int promoDay = promotionDayOfWeek ?? (promotionActiveNow == true ? (int)now.DayOfWeek : 0);
            TimeSpan promoTime;
            if (!string.IsNullOrWhiteSpace(promotionTime) && TimeSpan.TryParse(promotionTime, out var parsedPromoTime))
            {
                promoTime = parsedPromoTime;
            }
            else if (promotionActiveNow == true)
            {
                promoTime = now.TimeOfDay;
            }
            else
            {
                promoTime = new TimeSpan(12, 0, 0); // Meio-dia como padrão se não informado
            }

            var promoDayStr = ((DayOfWeek)promoDay).ToString().Substring(0, 3); // Ex: "Mon", "Tue"

            // Buscar promoções relevantes do banco (filtro por tenant e ativo)
            var promoQuery = _context.Promotions
                .Where(p => p.IsActive && p.CompanyId == companyId && p.MenuItemId != null && p.DaysOfWeek.Contains(promoDayStr));
            var promoList = await promoQuery.ToListAsync();

            // Filtrar promoções por horário em memória
            var validPromoMenuItemIds = promoList
                .Where(p => p.Hours.Split(',').Any(h =>
                {
                    var parts = h.Split('-');
                    if (parts.Length == 2 && TimeSpan.TryParse(parts[0], out var start) && TimeSpan.TryParse(parts[1], out var end))
                    {
                        return start <= promoTime && end > promoTime;
                    }
                    return false;
                }))
                .Select(p => p.MenuItemId!)
                .ToHashSet();

            query = query.Where(m => validPromoMenuItemIds.Contains(m.Id));
        }

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

    public async Task<MenuItem?> GetByIdAsync(string id, string companyId)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(companyId))
            throw new ArgumentException("Id e CompanyId são obrigatórios.");
        return await _context.MenuItems
            .Include(m => m.MenuItemTags)
            .ThenInclude(mt => mt.Tag)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && m.CompanyId == companyId);
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
        var existing = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == menuItem.Id && m.CompanyId == menuItem.CompanyId);
        if (existing == null)
            return;
        _context.Entry(existing).CurrentValues.SetValues(menuItem);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id, string companyId)
    {
        var menuItem = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == id && m.CompanyId == companyId);
        if (menuItem == null)
            return;
        _context.MenuItems.Remove(menuItem);
        await _context.SaveChangesAsync();
    }

    public async Task AddTagsAsync(string menuItemId, IEnumerable<string> tagIds, string companyId)
    {
        var menuItem = await _context.MenuItems.Include(m => m.MenuItemTags).FirstOrDefaultAsync(m => m.Id == menuItemId && m.CompanyId == companyId);
        if (menuItem == null)
            throw new ArgumentException("MenuItem não encontrado.");
        var tags = await _context.Tags.Where(t => tagIds.Contains(t.Id) && t.CompanyId == companyId).ToListAsync();
        menuItem.MenuItemTags = tags.Select(t => new MenuItemTag { MenuItemId = menuItemId, TagId = t.Id }).ToList();
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ValidateTagIdsAsync(IEnumerable<string> tagIds, string companyId)
    {
        var count = await _context.Tags.CountAsync(t => tagIds.Contains(t.Id) && t.CompanyId == companyId);
        return count == tagIds.Count();
    }

    public async Task<PagedResult<MenuItem>> GetAllAsync(
        string? companyId = null,
        List<string>? tagIds = null,
        List<string>? categoryIds = null,
        decimal? maxPrice = null,
        bool? isAvailable = null,
        bool? promotionActiveNow = null,
        int? promotionDayOfWeek = null,
        string? promotionTime = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc")
    {
        var query = _context.MenuItems
            .Include(m => m.MenuItemTags)
            .ThenInclude(mt => mt.Tag)
            .Include(m => m.MenuItemAdditionals)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(companyId))
            query = query.Where(m => m.CompanyId == companyId);
        if (tagIds != null && tagIds.Any())
            query = query.Where(m => m.MenuItemTags.Any(mt => tagIds.Contains(mt.TagId)));
        if (categoryIds != null && categoryIds.Any())
            query = query.Where(m => categoryIds.Contains(m.CategoryId));
        if (maxPrice.HasValue)
            query = query.Where(m => m.Price <= maxPrice.Value);
        if (isAvailable.HasValue)
            query = query.Where(m => m.IsAvailable == isAvailable.Value);

        // Filtro por promoções ativas
        if (promotionActiveNow == true || promotionDayOfWeek.HasValue || !string.IsNullOrWhiteSpace(promotionTime))
        {
            var now = DateTime.UtcNow;
            int promoDay = promotionDayOfWeek ?? (promotionActiveNow == true ? (int)now.DayOfWeek : 0);
            TimeSpan promoTime;
            if (!string.IsNullOrWhiteSpace(promotionTime) && TimeSpan.TryParse(promotionTime, out var parsedPromoTime))
            {
                promoTime = parsedPromoTime;
            }
            else if (promotionActiveNow == true)
            {
                promoTime = now.TimeOfDay;
            }
            else
            {
                promoTime = new TimeSpan(12, 0, 0); // Meio-dia como padrão se não informado
            }

            var promoDayStr = ((DayOfWeek)promoDay).ToString().Substring(0, 3); // Ex: "Mon", "Tue"

            var promoQuery = _context.Promotions
                .Where(p => p.IsActive && p.MenuItemId != null && p.DaysOfWeek.Contains(promoDayStr));
            var promoList = await promoQuery.ToListAsync();

            var validPromoMenuItemIds = promoList
                .Where(p => p.Hours.Split(',').Any(h =>
                {
                    var parts = h.Split('-');
                    if (parts.Length == 2 && TimeSpan.TryParse(parts[0], out var start) && TimeSpan.TryParse(parts[1], out var end))
                    {
                        return start <= promoTime && end > promoTime;
                    }
                    return false;
                }))
                .Select(p => p.MenuItemId!)
                .ToHashSet();

            query = query.Where(m => validPromoMenuItemIds.Contains(m.Id));
        }

        // Ordenação dinâmica
        if (!string.IsNullOrEmpty(sortBy))
        {
            if (sortOrder?.ToLower() == "desc")
                query = query.OrderByDescending(e => EF.Property<object>(e, sortBy));
            else
                query = query.OrderBy(e => EF.Property<object>(e, sortBy));
        }
        else
        {
            query = query.OrderByDescending(m => m.CreatedAt);
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

    public Task<PagedResult<MenuItem>> GetAllGlobalAsync(string? name = null, string? companyId = null, string? categoryId = null, bool? isAvailable = null, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        throw new NotImplementedException();
    }
} 
