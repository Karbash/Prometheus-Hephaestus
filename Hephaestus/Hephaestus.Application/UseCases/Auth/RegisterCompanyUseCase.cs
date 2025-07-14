using Hephaestus.Application.DTOs.Request;
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

    /// <summary>
    /// Inicializa uma nova instância do <see cref="RegisterCompanyUseCase"/>.
    /// </summary>
    /// <param name="companyRepository">Repositório de empresas.</param>
    /// <param name="auditLogRepository">Repositório de logs de auditoria.</param>
    /// <param name="loggedUserService">Serviço para obter informações do usuário logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public RegisterCompanyUseCase(
        ICompanyRepository companyRepository,
        IAuditLogRepository auditLogRepository,
        ILoggedUserService loggedUserService,
        ILogger<RegisterCompanyUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _companyRepository = companyRepository;
        _auditLogRepository = auditLogRepository;
        _loggedUserService = loggedUserService;
    }

    /// <summary>
    /// Executa o registro de uma nova empresa.
    /// </summary>
    /// <param name="request">Dados da empresa a ser registrada.</param>
    /// <param name="claimsPrincipal">Usuário autenticado.</param>
    /// <returns>ID da empresa registrada.</returns>
    public async Task<string> ExecuteAsync(RegisterCompanyRequest request, ClaimsPrincipal? claimsPrincipal)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação dos dados de entrada
            ValidateRequest(request);

            // Validação de autorização
            ValidateAuthorization(claimsPrincipal);

            // Validação das regras de negócio
            await ValidateBusinessRulesAsync(request);

            // Criação da empresa
            var company = await CreateCompanyEntityAsync(request);

            // Registro de auditoria
            await CreateAuditLogAsync(company, claimsPrincipal);

            return company.Id;
        });
    }

    /// <summary>
    /// Valida os dados da requisição.
    /// </summary>
    /// <param name="request">Requisição a ser validada.</param>
    private void ValidateRequest(RegisterCompanyRequest request)
    {
        if (request == null)
            throw new ValidationException("Dados da empresa são obrigatórios.", new ValidationResult());

        if (string.IsNullOrEmpty(request.Email))
            throw new ValidationException("E-mail é obrigatório.", new ValidationResult());

        if (string.IsNullOrEmpty(request.Password))
            throw new ValidationException("Senha é obrigatória.", new ValidationResult());

        if (string.IsNullOrEmpty(request.Name))
            throw new ValidationException("Nome da empresa é obrigatório.", new ValidationResult());

        if (string.IsNullOrEmpty(request.PhoneNumber))
            throw new ValidationException("Telefone é obrigatório.", new ValidationResult());
    }

    /// <summary>
    /// Valida a autorização do usuário.
    /// </summary>
    /// <param name="claimsPrincipal">Usuário autenticado.</param>
    private void ValidateAuthorization(ClaimsPrincipal? claimsPrincipal)
    {
        if (claimsPrincipal == null)
            throw new UnauthorizedException("Usuário não autenticado.", "REGISTER_COMPANY", "Company");

        var userRole = claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "Admin")
            throw new UnauthorizedException("Apenas administradores podem registrar empresas.", "REGISTER_COMPANY", "Company");
    }

    /// <summary>
    /// Valida as regras de negócio.
    /// </summary>
    /// <param name="request">Requisição com os dados.</param>
    private async Task ValidateBusinessRulesAsync(RegisterCompanyRequest request)
    {
        // Verifica se o e-mail já está registrado
        var existingByEmail = await _companyRepository.GetByEmailAsync(request.Email);
        if (existingByEmail != null)
            throw new ConflictException("E-mail já registrado.", "Company", "Email", request.Email);

        // Verifica se o telefone já está registrado
        var existingByPhone = await _companyRepository.GetByPhoneNumberAsync(request.PhoneNumber);
        if (existingByPhone != null)
            throw new ConflictException("Telefone já registrado.", "Company", "PhoneNumber", request.PhoneNumber);
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
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            ApiKey = request.ApiKey,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = Role.Tenant,
            IsEnabled = request.IsEnabled,
            FeeType = request.FeeType,
            FeeValue = request.FeeValue,
            State = request.State,
            City = request.City,
            Neighborhood = request.Neighborhood,
            Street = request.Street,
            Number = request.Number,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Slogan = request.Slogan,
            Description = request.Description
        };

        await _companyRepository.AddAsync(company);
        return company;
    }

    /// <summary>
    /// Cria o log de auditoria.
    /// </summary>
    /// <param name="company">Empresa criada.</param>
    /// <param name="claimsPrincipal">Usuário autenticado.</param>
    private async Task CreateAuditLogAsync(Domain.Entities.Company company, ClaimsPrincipal? claimsPrincipal)
    {
        var loggedUser = await _loggedUserService.GetLoggedUserAsync(claimsPrincipal!);
        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = loggedUser.Id,
            Action = "Registro de Empresa",
            EntityId = company.Id,
            Details = $"Empresa {company.Name} registrada com e-mail {company.Email} no estado {company.State}.",
            CreatedAt = DateTime.UtcNow
        });
    }
}