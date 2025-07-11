using Hephaestus.Application.DTOs.Response;

namespace Hephaestus.Application.Interfaces.Administration;

/// <summary>
/// Interface para o caso de uso de busca de empresas dentro de um raio.
/// </summary>
public interface IGetCompaniesWithinRadiusUseCase
{
    /// <summary>
    /// Busca empresas dentro de um raio a partir de uma coordenada.
    /// </summary>
    /// <param name="centerLat">Latitude do ponto central.</param>
    /// <param name="centerLon">Longitude do ponto central.</param>
    /// <param name="radiusKm">Raio em quilômetros.</param>
    /// <returns>Lista de empresas dentro do raio.</returns>
    /// <exception cref="System.Exception">Erro inesperado ao buscar empresas.</exception>
    Task<IEnumerable<CompanyResponse>> ExecuteAsync(double centerLat, double centerLon, double radiusKm);
}