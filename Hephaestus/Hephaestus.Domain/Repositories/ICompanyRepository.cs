using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories
{
    public interface ICompanyRepository
    {
        Task<PagedResult<Company>> GetAllAsync(bool? isEnabled, int pageNumber = 1, int pageSize = 20);
        Task<Company?> GetByIdAsync(string id);
        Task<Company?> GetByEmailAsync(string email);
        Task<Company?> GetByPhoneNumberAsync(string phoneNumber);
        Task AddAsync(Company company);
        Task UpdateAsync(Company company);
        Task<IEnumerable<Company>> GetCompaniesWithinRadiusAsync(double centerLat, double centerLon, double radiusKm, string? city = null, string? neighborhood = null, List<string>? tagIds = null, List<string>? categoryIds = null, decimal? maxPrice = null, bool? openNow = null, int? dayOfWeek = null, string? time = null, bool? promotionActiveNow = null, int? promotionDayOfWeek = null, string? promotionTime = null);
    }
} 
