using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Auth;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Enum;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Hephaestus.Application.Services;
using FluentValidation.Results;

namespace Hephaestus.Application.UseCases.Auth;

/// <summary>
/// Caso de uso para autenticação multifator (MFA).
/// </summary>
public class MfaUseCase : BaseUseCase, IMfaUseCase
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IConfiguration _configuration;
    private readonly IMfaService _mfaService;
    private readonly IAuditLogRepository _auditLogRepository;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="MfaUseCase"/>.
    /// </summary>
    /// <param name="companyRepository">Repositório de empresas.</param>
    /// <param name="configuration">Configuração da aplicação.</param>
    /// <param name="mfaService">Serviço de autenticação multifator.</param>
    /// <param name="auditLogRepository">Repositório de logs de auditoria.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public MfaUseCase(
        ICompanyRepository companyRepository,
        IConfiguration configuration,
        IMfaService mfaService,
        IAuditLogRepository auditLogRepository,
        ILogger<MfaUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _companyRepository = companyRepository;
        _configuration = configuration;
        _mfaService = mfaService;
        _auditLogRepository = auditLogRepository;
    }

    /// <summary>
    /// Valida um código MFA e retorna um novo token JWT com a claim MfaValidated.
    /// </summary>
    /// <param name="request">E-mail e código MFA.</param>
    /// <returns>Token JWT com MFA validado.</returns>
    public async Task<string> ExecuteAsync(MfaRequest request)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação dos dados de entrada
            ValidateRequest(request);

            // Busca e validação da empresa
            var company = await GetAndValidateCompanyAsync(request.Email);

            // Validação do código MFA
            await ValidateMfaCodeAsync(request.Email, request.MfaCode);

            // Registro de auditoria
            await CreateAuditLogAsync(company, "Validação MFA", $"MFA validado para {company.Email}.");

            // Geração do token JWT
            return await GenerateMfaTokenAsync(company);
        }, "Validação MFA");
    }

    /// <summary>
    /// Configura o MFA para um administrador, gerando um segredo TOTP.
    /// </summary>
    /// <param name="email">E-mail do administrador.</param>
    /// <returns>Segredo TOTP para configuração no aplicativo autenticador.</returns>
    public async Task<string> SetupMfaAsync(string email)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação dos dados de entrada
            ValidateEmail(email);

            // Busca e validação da empresa
            var company = await GetAndValidateCompanyAsync(email);

            // Geração do segredo MFA
            var secret = await _mfaService.GenerateMfaSecretAsync(email);

            // Registro de auditoria
            await CreateAuditLogAsync(company, "Configuração MFA", $"MFA configurado para {company.Email}.");

            return secret;
        }, "Configuração MFA");
    }

    /// <summary>
    /// Valida os dados da requisição MFA.
    /// </summary>
    /// <param name="request">Requisição a ser validada.</param>
    private void ValidateRequest(MfaRequest request)
    {
        if (request == null)
            throw new Hephaestus.Application.Exceptions.ValidationException("Dados de MFA são obrigatórios.", new ValidationResult());

        if (string.IsNullOrEmpty(request.Email))
            throw new Hephaestus.Application.Exceptions.ValidationException("E-mail é obrigatório.", new ValidationResult());

        if (string.IsNullOrEmpty(request.MfaCode))
            throw new Hephaestus.Application.Exceptions.ValidationException("Código MFA é obrigatório.", new ValidationResult());
    }

    /// <summary>
    /// Valida o e-mail fornecido.
    /// </summary>
    /// <param name="email">E-mail a ser validado.</param>
    private void ValidateEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            throw new Hephaestus.Application.Exceptions.ValidationException("E-mail é obrigatório.", new ValidationResult());
    }

    /// <summary>
    /// Busca e valida a empresa.
    /// </summary>
    /// <param name="email">E-mail da empresa.</param>
    /// <returns>Empresa encontrada.</returns>
    private async Task<Domain.Entities.Company> GetAndValidateCompanyAsync(string email)
    {
        var company = await _companyRepository.GetByEmailAsync(email);
        EnsureEntityExists(company, "Empresa", email);

        ValidateAuthorization(company.Role == Role.Admin, "Apenas administradores podem usar MFA.", "MFA", "Empresa");

        return company;
    }

    /// <summary>
    /// Valida o código MFA.
    /// </summary>
    /// <param name="email">E-mail da empresa.</param>
    /// <param name="mfaCode">Código MFA a ser validado.</param>
    private async Task ValidateMfaCodeAsync(string email, string mfaCode)
    {
        var isValid = await _mfaService.ValidateMfaCodeAsync(email, mfaCode);
        ValidateBusinessRule(isValid, "Código MFA inválido.", "MFA_VALIDATION");
    }

    /// <summary>
    /// Cria o log de auditoria.
    /// </summary>
    /// <param name="company">Empresa.</param>
    /// <param name="action">Ação realizada.</param>
    /// <param name="details">Detalhes da ação.</param>
    private async Task CreateAuditLogAsync(Domain.Entities.Company company, string action, string details)
    {
        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = company.Id,
            Action = action,
            EntityId = company.Id,
            Details = details,
            CreatedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Gera o token JWT com MFA validado.
    /// </summary>
    /// <param name="company">Empresa.</param>
    /// <returns>Token JWT.</returns>
    private Task<string> GenerateMfaTokenAsync(Domain.Entities.Company company)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured"));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, company.Id),
                new Claim(ClaimTypes.Name, company.Name),
                new Claim(ClaimTypes.Email, company.Email),
                new Claim(ClaimTypes.Role, company.Role.ToString()),
                new Claim("TenantId", string.Empty),
                new Claim("MfaValidated", "true")
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Task.FromResult(tokenHandler.WriteToken(token));
    }
}