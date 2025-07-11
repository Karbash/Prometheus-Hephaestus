using Hephaestus.Application.DTOs.Response;

namespace Hephaestus.Application.Interfaces.Administration;

public interface IGetCompaniesWithinRadiusUseCase
{
    Task<IEnumerable<CompanyResponse>> ExecuteAsync(double centerLat, double centerLon, double radiusKm, string? city = null, string? neighborhood = null);
}