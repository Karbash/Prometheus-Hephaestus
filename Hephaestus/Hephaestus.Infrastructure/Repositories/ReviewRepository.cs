using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Hephaestus.Domain.DTOs.Response;

namespace Hephaestus.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly HephaestusDbContext _context;

    public ReviewRepository(HephaestusDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Review review)
    {
        review.SetStarsFromRating();
        await _context.Reviews.AddAsync(review);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Review>> GetByOrderIdAsync(string orderId)
    {
        return await _context.Reviews.Where(r => r.OrderId == orderId).ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetByTenantIdAsync(string tenantId)
    {
        return await _context.Reviews.Where(r => r.TenantId == tenantId).ToListAsync();
    }

    public async Task<PagedResult<Review>> GetAllGlobalAsync(
        string? companyId = null,
        string? customerId = null,
        string? orderId = null,
        int? ratingMin = null,
        int? ratingMax = null,
        bool? isActive = null,
        DateTime? dataInicial = null,
        DateTime? dataFinal = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc")
    {
        var query = _context.Reviews.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(companyId))
            query = query.Where(r => r.TenantId == companyId);
        if (!string.IsNullOrEmpty(customerId))
            query = query.Where(r => r.CustomerId == customerId);
        if (!string.IsNullOrEmpty(orderId))
            query = query.Where(r => r.OrderId == orderId);
        if (ratingMin.HasValue)
            query = query.Where(r => r.Rating >= ratingMin.Value);
        if (ratingMax.HasValue)
            query = query.Where(r => r.Rating <= ratingMax.Value);
        // Removido filtro por isActive, pois Review não possui essa propriedade
        if (dataInicial.HasValue)
            query = query.Where(r => r.CreatedAt >= dataInicial.Value);
        if (dataFinal.HasValue)
            query = query.Where(r => r.CreatedAt <= dataFinal.Value);

        // Ordenação dinâmica
        if (!string.IsNullOrEmpty(sortBy))
        {
            query = sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                : query.OrderBy(e => EF.Property<object>(e, sortBy));
        }
        else
        {
            query = query.OrderByDescending(r => r.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Review>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
} 