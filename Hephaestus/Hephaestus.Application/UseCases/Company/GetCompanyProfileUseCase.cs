using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Interfaces.Company;
using Hephaestus.Domain.Repositories;
using System.Threading.Tasks;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using FluentValidation.Results;

namespace Hephaestus.Application.UseCases.Company;

/// <summary>
/// Caso de uso para obter o perfil completo de uma empresa.
/// </summary>
public class GetCompanyProfileUseCase : BaseUseCase, IGetCompanyProfileUseCase
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ICompanyImageRepository _companyImageRepository;
    private readonly ICompanyOperatingHourRepository _companyOperatingHourRepository;
    private readonly ICompanySocialMediaRepository _companySocialMediaRepository;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="GetCompanyProfileUseCase"/>.
    /// </summary>
    /// <param name="companyRepository">Reposit�rio de empresas.</param>
    /// <param name="companyImageRepository">Reposit�rio de imagens da empresa.</param>
    /// <param name="companyOperatingHourRepository">Reposit�rio de hor�rios de funcionamento.</param>
    /// <param name="companySocialMediaRepository">Reposit�rio de redes sociais.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
    public GetCompanyProfileUseCase(
        ICompanyRepository companyRepository,
        ICompanyImageRepository companyImageRepository,
        ICompanyOperatingHourRepository companyOperatingHourRepository,
        ICompanySocialMediaRepository companySocialMediaRepository,
        ILogger<GetCompanyProfileUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _companyRepository = companyRepository;
        _companyImageRepository = companyImageRepository;
        _companyOperatingHourRepository = companyOperatingHourRepository;
        _companySocialMediaRepository = companySocialMediaRepository;
    }

    /// <summary>
    /// Executa a busca do perfil completo de uma empresa.
    /// </summary>
    /// <param name="companyId">ID da empresa.</param>
    /// <returns>Perfil completo da empresa.</returns>
    public async Task<CompanyProfileResponse> ExecuteAsync(string companyId)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Valida��o dos par�metros de entrada
            ValidateInputParameters(companyId);

            // Busca e valida��o da empresa
            var company = await GetAndValidateCompanyAsync(companyId);

            // Busca dos dados relacionados
            var relatedData = await GetRelatedDataAsync(companyId);

            // Montagem da resposta
            return BuildCompanyProfileResponse(company, relatedData);
        });
    }

    /// <summary>
    /// Valida os par�metros de entrada.
    /// </summary>
    /// <param name="companyId">ID da empresa.</param>
    private void ValidateInputParameters(string companyId)
    {
        if (string.IsNullOrEmpty(companyId))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID da empresa � obrigat�rio.", new ValidationResult());

        if (!Guid.TryParse(companyId, out _))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID da empresa deve ser um GUID v�lido.", new ValidationResult());
    }

    /// <summary>
    /// Busca e valida a empresa.
    /// </summary>
    /// <param name="companyId">ID da empresa.</param>
    /// <returns>Empresa encontrada.</returns>
    private async Task<Domain.Entities.Company> GetAndValidateCompanyAsync(string companyId)
    {
        var company = await _companyRepository.GetByIdAsync(companyId);
        if (company == null)
            throw new NotFoundException("Empresa", companyId);

        return company;
    }

    /// <summary>
    /// Busca os dados relacionados da empresa.
    /// </summary>
    /// <param name="companyId">ID da empresa.</param>
    /// <returns>Dados relacionados.</returns>
    private async Task<(IEnumerable<Domain.Entities.CompanyImage> images, 
                       IEnumerable<Domain.Entities.CompanyOperatingHour> operatingHours, 
                       IEnumerable<Domain.Entities.CompanySocialMedia> socialMedia)> GetRelatedDataAsync(string companyId)
    {
        var images = await _companyImageRepository.GetByCompanyIdAsync(companyId);
        var operatingHours = await _companyOperatingHourRepository.GetByCompanyIdAsync(companyId);
        var socialMedia = await _companySocialMediaRepository.GetByCompanyIdAsync(companyId);

        return (images, operatingHours, socialMedia);
    }

    /// <summary>
    /// Monta a resposta do perfil da empresa.
    /// </summary>
    /// <param name="company">Empresa.</param>
    /// <param name="relatedData">Dados relacionados.</param>
    /// <returns>Perfil da empresa.</returns>
    private CompanyProfileResponse BuildCompanyProfileResponse(
        Domain.Entities.Company company,
        (IEnumerable<Domain.Entities.CompanyImage> images, 
         IEnumerable<Domain.Entities.CompanyOperatingHour> operatingHours, 
         IEnumerable<Domain.Entities.CompanySocialMedia> socialMedia) relatedData)
    {
        // Nunca expor PasswordHash, ApiKey ou MfaSecret em DTOs de resposta. Se adicionar novos campos sens�veis, garantir que n�o sejam expostos aqui.
        return new CompanyProfileResponse
        {
            Id = company.Id,
            Name = company.Name,
            Email = company.Email,
            PhoneNumber = company.PhoneNumber,
            Slogan = company.Slogan,
            Description = company.Description,
            Images = relatedData.images.Select(i => new CompanyImageResponse
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                ImageType = i.ImageType
            }).ToList(),
            OperatingHours = relatedData.operatingHours.Select(oh => new CompanyOperatingHourResponse
            {
                DayOfWeek = oh.DayOfWeek,
                OpenTime = oh.OpenTime,
                CloseTime = oh.CloseTime,
                IsClosed = oh.IsClosed
            }).ToList(),
            SocialMedia = relatedData.socialMedia.Select(sm => new CompanySocialMediaResponse
            {
                Platform = sm.Platform,
                Url = sm.Url
            }).ToList()
        };
    }
}
