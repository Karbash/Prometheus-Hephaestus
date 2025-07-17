using Hephaestus.Domain.Entities;
using Hephaestus.Domain.DTOs.Response;

namespace Hephaestus.Domain.Repositories;

public interface IReviewRepository
{
    Task AddAsync(Review review);
    Task<IEnumerable<Review>> GetByOrderIdAsync(string orderId);
    Task<IEnumerable<Review>> GetByTenantIdAsync(string tenantId);
    Task<PagedResult<Review>> GetAllGlobalAsync(
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
        string? sortOrder = "asc");
} 