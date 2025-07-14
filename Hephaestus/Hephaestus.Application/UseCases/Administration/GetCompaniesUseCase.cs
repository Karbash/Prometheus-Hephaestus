using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;

namespace Hephaestus.Application.UseCases.Administration;

/// <summary>
/// Caso de uso para obter todas as empresas.
/// </summary>
public class GetCompaniesUseCase : BaseUseCase, IGetCompaniesUseCase
{
    private readonly ICompanyRepository _companyRepository;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="GetCompaniesUseCase"/>.
    /// </summary>
    /// <param name="companyRepository">Repositório de empresas.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public GetCompaniesUseCase(
        ICompanyRepository companyRepository,
        ILogger<GetCompaniesUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _companyRepository = companyRepository;
    }

    /// <summary>
    /// Executa a busca de todas as empresas.
    /// </summary>
    /// <param name="isEnabled">Filtro opcional para empresas habilitadas.</param>
    /// <returns>Lista de empresas.</returns>
    public async Task<IEnumerable<CompanyResponse>> ExecuteAsync(bool? isEnabled)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Busca das empresas
            var companies = await GetCompaniesAsync(isEnabled);

            // Conversão para DTOs de resposta
            return ConvertToResponseDtos(companies);
        });
    }

    /// <summary>
    /// Busca as empresas.
    /// </summary>
    /// <param name="isEnabled">Filtro opcional para empresas habilitadas.</param>
    /// <returns>Lista de empresas.</returns>
    private async Task<IEnumerable<Domain.Entities.Company>> GetCompaniesAsync(bool? isEnabled)
    {
        return await _companyRepository.GetAllAsync(isEnabled);
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
            FeeType = c.FeeType.ToString(),
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