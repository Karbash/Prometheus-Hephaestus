using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Administration;

/// <summary>
/// Caso de uso para buscar empresas dentro de um raio a partir de uma coordenada.
/// </summary>
public class GetCompaniesWithinRadiusUseCase : IGetCompaniesWithinRadiusUseCase
{
    private readonly ICompanyRepository _companyRepository;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="GetCompaniesWithinRadiusUseCase"/>.
    /// </summary>
    /// <param name="companyRepository">Repositório de empresas.</param>
    public GetCompaniesWithinRadiusUseCase(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    /// <summary>
    /// Busca empresas dentro de um raio a partir de uma coordenada.
    /// </summary>
    /// <param name="centerLat">Latitude do ponto central.</param>
    /// <param name="centerLon">Longitude do ponto central.</param>
    /// <param name="radiusKm">Raio em quilômetros.</param>
    /// <returns>Lista de empresas dentro do raio.</returns>
    /// <exception cref="ArgumentException">Se os parâmetros de latitude, longitude ou raio forem inválidos.</exception>
    public async Task<IEnumerable<CompanyResponse>> ExecuteAsync(double centerLat, double centerLon, double radiusKm)
    {
        if (centerLat < -90 || centerLat > 90)
            throw new ArgumentException("Latitude deve estar entre -90 e 90 graus.", nameof(centerLat));
        if (centerLon < -180 || centerLon > 180)
            throw new ArgumentException("Longitude deve estar entre -180 e 180 graus.", nameof(centerLon));
        if (radiusKm <= 0)
            throw new ArgumentException("Raio deve ser maior que zero.", nameof(radiusKm));

        var companies = await _companyRepository.GetCompaniesWithinRadiusAsync(centerLat, centerLon, radiusKm);
        return companies.Select(c => new CompanyResponse
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email,
            PhoneNumber = c.PhoneNumber,
            IsEnabled = c.IsEnabled,
            FeeType = c.FeeType.ToString(),
            FeeValue = (double)c.FeeValue,
            City = c.City,
            Street = c.Street,
            Number = c.Number,
            Latitude = c.Latitude,
            Longitude = c.Longitude,
            Slogan = c.Slogan,
            Description = c.Description
        });
    }
}