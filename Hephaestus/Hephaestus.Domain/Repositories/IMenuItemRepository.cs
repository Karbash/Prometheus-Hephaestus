using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories
{
    public interface IMenuItemRepository
    {
        Task<PagedResult<MenuItem>> GetByCompanyIdAsync(string companyId, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc", List<string>? tagIds = null, List<string>? categoryIds = null, decimal? maxPrice = null, bool? promotionActiveNow = null, int? promotionDayOfWeek = null, string? promotionTime = null);
        Task<MenuItem?> GetByIdAsync(string id, string companyId);
        Task AddAsync(MenuItem menuItem);
        Task UpdateAsync(MenuItem menuItem);
        Task DeleteAsync(string id, string companyId);
        Task AddTagsAsync(string menuItemId, IEnumerable<string> tagIds, string companyId);
        Task<bool> ValidateTagIdsAsync(IEnumerable<string> tagIds, string companyId);
        Task<PagedResult<MenuItem>> GetAllGlobalAsync(string? name = null, string? companyId = null, string? categoryId = null, bool? isAvailable = null, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
    }
} 
