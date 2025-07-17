using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
} 