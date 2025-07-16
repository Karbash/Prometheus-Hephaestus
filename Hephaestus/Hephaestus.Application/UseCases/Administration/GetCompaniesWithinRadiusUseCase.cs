using FluentValidation.Results;
using Hephaestus.Application.Base;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Enum;
using Hephaestus.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Hephaestus.Application.UseCases.Administration;

/// <summary>
/// Caso de uso para obter empresas dentro de um raio específico.
/// </summary>
public class GetCompaniesWithinRadiusUseCase : BaseUseCase, IGetCompaniesWithinRadiusUseCase
{
    private readonly ICompanyRepository _companyRepository;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="GetCompaniesWithinRadiusUseCase"/>.
    /// </summary>
    /// <param name="companyRepository">Repositório de empresas.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public GetCompaniesWithinRadiusUseCase(
        ICompanyRepository companyRepository,
        ILogger<GetCompaniesWithinRadiusUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _companyRepository = companyRepository;
    }

    /// <summary>
    /// Executa a busca de empresas dentro de um raio específico.
    /// </summary>
    /// <param name="centerLat">Latitude do centro.</param>
    /// <param name="centerLon">Longitude do centro.</param>
    /// <param name="radiusKm">Raio em quilômetros.</param>
    /// <param name="city">Filtro opcional por cidade.</param>
    /// <param name="neighborhood">Filtro opcional por bairro.</param>
    /// <returns>Lista de empresas dentro do raio.</returns>
    public async Task<IEnumerable<CompanyResponse>> ExecuteAsync(double centerLat, double centerLon, double radiusKm, string? city = null, string? neighborhood = null)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação dos parâmetros de entrada
            ValidateInputParameters(centerLat, centerLon, radiusKm, city, neighborhood);

            // Busca das empresas
            var companies = await GetCompaniesAsync(centerLat, centerLon, radiusKm, city, neighborhood);

            // Conversão para DTOs de resposta
            return ConvertToResponseDtos(companies);
        });
    }

    /// <summary>
    /// Valida os parâmetros de entrada.
    /// </summary>
    /// <param name="centerLat">Latitude do centro.</param>
    /// <param name="centerLon">Longitude do centro.</param>
    /// <param name="radiusKm">Raio em quilômetros.</param>
    /// <param name="city">Filtro opcional por cidade.</param>
    /// <param name="neighborhood">Filtro opcional por bairro.</param>
    private void ValidateInputParameters(double centerLat, double centerLon, double radiusKm, string? city, string? neighborhood)
    {
        if (centerLat < -90 || centerLat > 90)
            throw new Hephaestus.Application.Exceptions.ValidationException("Latitude deve estar entre -90 e 90 graus.", new ValidationResult());

        if (centerLon < -180 || centerLon > 180)
            throw new Hephaestus.Application.Exceptions.ValidationException("Longitude deve estar entre -180 e 180 graus.", new ValidationResult());

        if (radiusKm <= 0)
            throw new Hephaestus.Application.Exceptions.ValidationException("Raio deve ser maior que zero.", new ValidationResult());

        if (city != null && string.IsNullOrWhiteSpace(city))
            throw new Hephaestus.Application.Exceptions.ValidationException("Cidade não pode ser vazia.", new ValidationResult());

        if (neighborhood != null && string.IsNullOrWhiteSpace(neighborhood))
            throw new Hephaestus.Application.Exceptions.ValidationException("Bairro não pode ser vazio.", new ValidationResult());
    }

    /// <summary>
    /// Busca as empresas dentro do raio.
    /// </summary>
    /// <param name="centerLat">Latitude do centro.</param>
    /// <param name="centerLon">Longitude do centro.</param>
    /// <param name="radiusKm">Raio em quilômetros.</param>
    /// <param name="city">Filtro opcional por cidade.</param>
    /// <param name="neighborhood">Filtro opcional por bairro.</param>
    /// <returns>Lista de empresas.</returns>
    private async Task<IEnumerable<Domain.Entities.Company>> GetCompaniesAsync(double centerLat, double centerLon, double radiusKm, string? city, string? neighborhood)
    {
        return await _companyRepository.GetCompaniesWithinRadiusAsync(centerLat, centerLon, radiusKm, city, neighborhood);
    }

    /// <summary>
    /// Converte as entidades para DTOs de resposta.
    /// </summary>
    /// <param name="companies">Lista de empresas.</param>
    /// <returns>Lista de DTOs de resposta.</returns>
    private IEnumerable<CompanyResponse> ConvertToResponseDtos(IEnumerable<Domain.Entities.Company> companies)
    {
        return companies.Select(c => new CompanyResponse
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email,
            PhoneNumber = c.PhoneNumber,
            IsEnabled = c.IsEnabled,
            FeeType = c.FeeType,
            FeeValue = (double)c.FeeValue,
            State = c.State,
            City = c.City,
            Neighborhood = c.Neighborhood,
            Street = c.Street,
            Number = c.Number,
            Latitude = c.Latitude,
            Longitude = c.Longitude,
            Slogan = c.Slogan,
            Description = c.Description
        });
    }
}
