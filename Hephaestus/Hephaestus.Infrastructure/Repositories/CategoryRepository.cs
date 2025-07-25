using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hephaestus.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly HephaestusDbContext _context;
    private readonly ILogger<CategoryRepository> _logger;

    public CategoryRepository(HephaestusDbContext context, ILogger<CategoryRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Category?> GetByIdAsync(string id, string tenantId)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("Id e TenantId são obrigatórios.");

        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
    }

    public async Task<Category?> GetByNameAsync(string name, string? tenantId = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name é obrigatório.");
        
        if (tenantId == null)
        {
            // Busca categoria global
            return await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Name == name && c.IsGlobal);
        }
        else
        {
            // Busca categoria local
            return await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Name == name && c.TenantId == tenantId);
        }
    }

    public async Task<PagedResult<Category>> GetByTenantIdAsync(string tenantId, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("TenantId é obrigatório.");

        var query = _context.Categories.AsNoTracking().Where(c => c.TenantId == tenantId);

        if (!string.IsNullOrEmpty(sortBy))
        {
            if (sortOrder?.ToLower() == "desc")
                query = query.OrderByDescending(e => EF.Property<object>(e, sortBy));
            else
                query = query.OrderBy(e => EF.Property<object>(e, sortBy));
        }

        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<Category>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<Category>> GetAllAsync(
        string? companyId = null,
        string? name = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc")
    {
        var query = _context.Categories.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(companyId))
            ; // Não filtrar por CompanyId, pois não existe na entidade Category
        if (!string.IsNullOrEmpty(name))
            query = query.Where(c => c.Name.Contains(name));
        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        if (!string.IsNullOrEmpty(sortBy))
        {
            if (sortOrder?.ToLower() == "desc")
                query = query.OrderByDescending(e => EF.Property<object>(e, sortBy));
            else
                query = query.OrderBy(e => EF.Property<object>(e, sortBy));
        }
        else
        {
            query = query.OrderByDescending(c => c.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<Category>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<Category>> GetAllActiveAsync(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("TenantId é obrigatório.");

        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task AddAsync(Category category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id, string tenantId)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("Id e TenantId são obrigatórios.");

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);

        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string id, string tenantId)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(tenantId))
            return false;

        return await _context.Categories
            .AnyAsync(c => c.Id == id && c.TenantId == tenantId);
    }

    public async Task<bool> ExistsByNameAsync(string name, string tenantId)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(tenantId))
            return false;

        return await _context.Categories
            .AnyAsync(c => c.Name == name && c.TenantId == tenantId);
    }

    public async Task<PagedResult<Category>> GetAllGlobalAsync(string? name = null, string? companyId = null, bool? isActive = null, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        var query = _context.Categories.AsNoTracking().Where(c => c.IsGlobal);

        if (!string.IsNullOrEmpty(name))
            query = query.Where(c => c.Name.Contains(name));
        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        if (!string.IsNullOrEmpty(sortBy))
        {
            if (sortOrder?.ToLower() == "desc")
                query = query.OrderByDescending(e => EF.Property<object>(e, sortBy));
            else
                query = query.OrderBy(e => EF.Property<object>(e, sortBy));
        }
        else
        {
            query = query.OrderByDescending(c => c.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<Category>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<Category>> GetHybridCategoriesAsync(string tenantId, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("TenantId é obrigatório.");

        // Busca categorias locais da empresa + categorias globais
        var query = _context.Categories.AsNoTracking().Where(c => c.TenantId == tenantId || c.IsGlobal);

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
            query = query.OrderBy(c => !c.IsGlobal).ThenBy(c => c.Name);
        }

        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<Category>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
} 
