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
/// Caso de uso para obter todas as empresas.
/// </summary>
public class GetCompaniesUseCase : BaseUseCase, IGetCompaniesUseCase
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IAddressRepository _addressRepository;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="GetCompaniesUseCase"/>.
    /// </summary>
    /// <param name="companyRepository">Reposit�rio de empresas.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
    public GetCompaniesUseCase(
        ICompanyRepository companyRepository,
        ILogger<GetCompaniesUseCase> logger,
        IExceptionHandlerService exceptionHandler,
        IAddressRepository addressRepository)
        : base(logger, exceptionHandler)
    {
        _companyRepository = companyRepository;
        _addressRepository = addressRepository;
    }

    /// <summary>
    /// Executa a busca de todas as empresas.
    /// </summary>
    /// <param name="isEnabled">Filtro opcional para empresas habilitadas.</param>
    /// <returns>Lista de empresas.</returns>
    public async Task<PagedResult<CompanyResponse>> ExecuteAsync(bool? isEnabled, int pageNumber = 1, int pageSize = 20)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var pagedCompanies = await GetCompaniesAsync(isEnabled, pageNumber, pageSize);
            var companyResponses = new List<CompanyResponse>();
            foreach (var c in pagedCompanies.Items)
            {
                var address = (await _addressRepository.GetByEntityAsync(c.Id, "Company")).FirstOrDefault();
                companyResponses.Add(new CompanyResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    IsEnabled = c.IsEnabled,
                    FeeType = c.FeeType,
                    FeeValue = (double)c.FeeValue,
                    Slogan = c.Slogan,
                    Description = c.Description,
                    Address = address != null ? new AddressResponse
                    {
                        Id = address.Id,
                        Street = address.Street,
                        Number = address.Number,
                        Complement = address.Complement,
                        Neighborhood = address.Neighborhood,
                        City = address.City,
                        State = address.State,
                        ZipCode = address.ZipCode,
                        Reference = address.Reference,
                        Notes = address.Notes,
                        Latitude = address.Latitude,
                        Longitude = address.Longitude
                    } : null
                });
            }
            return new PagedResult<CompanyResponse>
            {
                Items = companyResponses,
                TotalCount = pagedCompanies.TotalCount,
                PageNumber = pagedCompanies.PageNumber,
                PageSize = pagedCompanies.PageSize
            };
        });
    }

    /// <summary>
    /// Busca as empresas.
    /// </summary>
    /// <param name="isEnabled">Filtro opcional para empresas habilitadas.</param>
    /// <returns>Lista de empresas.</returns>
    private async Task<PagedResult<Domain.Entities.Company>> GetCompaniesAsync(bool? isEnabled, int pageNumber, int pageSize)
    {
        return await _companyRepository.GetAllAsync(isEnabled, pageNumber, pageSize);
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
            Slogan = c.Slogan,
            Description = c.Description
        });
    }
}
