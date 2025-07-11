using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Administration;

public class GetCompaniesWithinRadiusUseCase : IGetCompaniesWithinRadiusUseCase
{
    private readonly ICompanyRepository _companyRepository;

    public GetCompaniesWithinRadiusUseCase(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<IEnumerable<CompanyResponse>> ExecuteAsync(double centerLat, double centerLon, double radiusKm, string? city = null, string? neighborhood = null)
    {
        if (centerLat < -90 || centerLat > 90)
            throw new ArgumentException("Latitude deve estar entre -90 e 90 graus.", nameof(centerLat));
        if (centerLon < -180 || centerLon > 180)
            throw new ArgumentException("Longitude deve estar entre -180 e 180 graus.", nameof(centerLon));
        if (radiusKm <= 0)
            throw new ArgumentException("Raio deve ser maior que zero.", nameof(radiusKm));
        if (city != null && string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("Cidade não pode ser vazia.", nameof(city));
        if (neighborhood != null && string.IsNullOrWhiteSpace(neighborhood))
            throw new ArgumentException("Bairro não pode ser vazio.", nameof(neighborhood));

        var companies = await _companyRepository.GetCompaniesWithinRadiusAsync(centerLat, centerLon, radiusKm, city, neighborhood);
        return companies.Select(c => new CompanyResponse
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email,
            PhoneNumber = c.PhoneNumber,
            IsEnabled = c.IsEnabled,
            FeeType = c.FeeType.ToString(),
            FeeValue = (double)c.FeeValue,
            State = c.State,
            City = c.City,
            Neighborhood = c.Neighborhood, // Novo campo
            Street = c.Street,
            Number = c.Number,
            Latitude = c.Latitude,
            Longitude = c.Longitude,
            Slogan = c.Slogan,
            Description = c.Description
        });
    }
}