using Hephaestus.Domain.DTOs.Response;

namespace Hephaestus.Application.Interfaces.Administration;

public interface IGetCompaniesWithinRadiusUseCase
{
    Task<IEnumerable<CompanyResponse>> ExecuteAsync(double centerLat, double centerLon, double radiusKm, string? city = null, string? neighborhood = null, List<string>? tagIds = null, List<string>? categoryIds = null, decimal? maxPrice = null, bool? openNow = null, int? dayOfWeek = null, string? time = null, bool? promotionActiveNow = null, int? promotionDayOfWeek = null, string? promotionTime = null);
}
