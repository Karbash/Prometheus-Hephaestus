using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories;

public interface IReviewRepository
{
    Task AddAsync(Review review);
    Task<IEnumerable<Review>> GetByOrderIdAsync(string orderId);
    Task<IEnumerable<Review>> GetByTenantIdAsync(string tenantId);
} 