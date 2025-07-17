using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Application.Interfaces.Auth;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Enum;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using System.Security.Claims;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using FluentValidation.Results;

namespace Hephaestus.Application.UseCases.Auth;

/// <summary>
/// Caso de uso para registro de empresas.
/// </summary>
public class RegisterCompanyUseCase : BaseUseCase, IRegisterCompanyUseCase
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILoggedUserService _loggedUserService;
    private readonly IAddressRepository _addressRepository;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="RegisterCompanyUseCase"/>.
    /// </summary>
    /// <param name="companyRepository">Reposit�rio de empresas.</param>
    /// <param name="auditLogRepository">Reposit�rio de logs de auditoria.</param>
    /// <param name="loggedUserService">Servi�o para obter informa��es do usu�rio logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
    public RegisterCompanyUseCase(
        ICompanyRepository companyRepository,
        IAuditLogRepository auditLogRepository,
        ILoggedUserService loggedUserService,
        IAddressRepository addressRepository,
        ILogger<RegisterCompanyUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _companyRepository = companyRepository;
        _auditLogRepository = auditLogRepository;
        _loggedUserService = loggedUserService;
        _addressRepository = addressRepository;
    }

    /// <summary>
    /// Executa o registro de uma nova empresa.
    /// </summary>
    /// <param name="request">Dados da empresa a ser registrada.</param>
    /// <param name="claimsPrincipal">Usu�rio autenticado.</param>
    /// <returns>ID da empresa registrada.</returns>
    public async Task<string> ExecuteAsync(RegisterCompanyRequest request, ClaimsPrincipal? claimsPrincipal)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Valida��o dos dados de entrada
            // Remover todas as linhas que usam _validator

            // Valida��o de autoriza��o
            ValidateAuthorization(claimsPrincipal);

            // Valida��o das regras de neg�cio
            await ValidateBusinessRulesAsync(request);

            // Cria��o da empresa
            var company = await CreateCompanyEntityAsync(request);

            // Registro de auditoria
            await CreateAuditLogAsync(company, claimsPrincipal);

            return company.Id;
        });
    }

    /// <summary>
    /// Valida a autoriza��o do usu�rio.
    /// </summary>
    /// <param name="claimsPrincipal">Usu�rio autenticado.</param>
    private void ValidateAuthorization(ClaimsPrincipal? claimsPrincipal)
    {
        if (claimsPrincipal == null)
            throw new UnauthorizedException("Usu�rio n�o autenticado.", "REGISTER_COMPANY", "Company");

        var userRole = claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "Admin")
            throw new UnauthorizedException("Apenas administradores podem registrar empresas.", "REGISTER_COMPANY", "Company");
    }

    /// <summary>
    /// Valida as regras de neg�cio.
    /// </summary>
    /// <param name="request">Requisi��o com os dados.</param>
    private async Task ValidateBusinessRulesAsync(RegisterCompanyRequest request)
    {
        // Verifica se o e-mail j� est� registrado
        var existingByEmail = await _companyRepository.GetByEmailAsync(request.Email);
        if (existingByEmail != null)
            throw new ConflictException("E-mail j� registrado.", "Company", "Email", request.Email);

        // Verifica se o telefone j� est� registrado
        if (!string.IsNullOrEmpty(request.PhoneNumber))
        {
            var existingByPhone = await _companyRepository.GetByPhoneNumberAsync(request.PhoneNumber);
            if (existingByPhone != null)
                throw new ConflictException("Telefone j� registrado.", "Company", "PhoneNumber", request.PhoneNumber);
        }
    }

    /// <summary>
    /// Cria a entidade de empresa.
    /// </summary>
    /// <param name="request">Dados da empresa.</param>
    /// <returns>Entidade de empresa criada.</returns>
    private async Task<Domain.Entities.Company> CreateCompanyEntityAsync(RegisterCompanyRequest request)
    {
        var company = new Domain.Entities.Company
        {
            Name = request.Name,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            ApiKey = request.ApiKey,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = Role.Tenant,
            IsEnabled = request.IsEnabled,
            FeeType = request.FeeType,
            FeeValue = (decimal)request.FeeValue,
            Slogan = request.Slogan,
            Description = request.Description
        };

        await _companyRepository.AddAsync(company);

        // Endereço
        var address = new Hephaestus.Domain.Entities.Address
        {
            TenantId = company.Id,
            EntityId = company.Id,
            EntityType = "Company",
            Street = request.Address.Street,
            Number = request.Address.Number,
            Complement = request.Address.Complement,
            Neighborhood = request.Address.Neighborhood,
            City = request.Address.City,
            State = request.Address.State,
            ZipCode = request.Address.ZipCode,
            Reference = request.Address.Reference,
            Notes = request.Address.Notes,
            Latitude = request.Address.Latitude ?? 0,
            Longitude = request.Address.Longitude ?? 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _addressRepository.AddAsync(address);

        return company;
    }

    /// <summary>
    /// Cria o log de auditoria.
    /// </summary>
    /// <param name="company">Empresa criada.</param>
    /// <param name="claimsPrincipal">Usu�rio autenticado.</param>
    private async Task CreateAuditLogAsync(Domain.Entities.Company company, ClaimsPrincipal? claimsPrincipal)
    {
        var loggedUser = await _loggedUserService.GetLoggedUserAsync(claimsPrincipal!);
        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = loggedUser.Id,
            Action = "Registro de Empresa",
            EntityId = company.Id,
            Details = $"Empresa {company.Name} registrada com e-mail {company.Email}.",
            CreatedAt = DateTime.UtcNow
        });
    }
}
