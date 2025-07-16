using Hephaestus.Domain.DTOs.Request;
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
/// Caso de uso para autentica��o multifator (MFA).
/// </summary>
public class MfaUseCase : BaseUseCase, IMfaUseCase
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IConfiguration _configuration;
    private readonly IMfaService _mfaService;
    private readonly IAuditLogRepository _auditLogRepository;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="MfaUseCase"/>.
    /// </summary>
    /// <param name="companyRepository">Reposit�rio de empresas.</param>
    /// <param name="configuration">Configura��o da aplica��o.</param>
    /// <param name="mfaService">Servi�o de autentica��o multifator.</param>
    /// <param name="auditLogRepository">Reposit�rio de logs de auditoria.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
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
    /// Valida um c�digo MFA e retorna um novo token JWT com a claim MfaValidated.
    /// </summary>
    /// <param name="request">E-mail e c�digo MFA.</param>
    /// <returns>Token JWT com MFA validado.</returns>
    public async Task<string> ExecuteAsync(MfaRequest request)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Valida��o dos dados de entrada
            ValidateRequest(request);

            // Busca e valida��o da empresa
            var company = await GetAndValidateCompanyAsync(request.Email);

            // Valida��o do c�digo MFA
            await ValidateMfaCodeAsync(request.Email, request.MfaCode);

            // Registro de auditoria
            await CreateAuditLogAsync(company, "Valida��o MFA", $"MFA validado para {company.Email}.");

            // Gera��o do token JWT
            return await GenerateMfaTokenAsync(company);
        }, "Valida��o MFA");
    }

    /// <summary>
    /// Configura o MFA para um administrador, gerando um segredo TOTP.
    /// </summary>
    /// <param name="email">E-mail do administrador.</param>
    /// <returns>Segredo TOTP para configura��o no aplicativo autenticador.</returns>
    public async Task<string> SetupMfaAsync(string email)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Valida��o dos dados de entrada
            ValidateEmail(email);

            // Busca e valida��o da empresa
            var company = await GetAndValidateCompanyAsync(email);

            // Gera��o do segredo MFA
            var secret = await _mfaService.GenerateMfaSecretAsync(email);

            // Registro de auditoria
            await CreateAuditLogAsync(company, "Configura��o MFA", $"MFA configurado para {company.Email}.");

            return secret;
        }, "Configura��o MFA");
    }

    /// <summary>
    /// Valida os dados da requisi��o MFA.
    /// </summary>
    /// <param name="request">Requisi��o a ser validada.</param>
    private void ValidateRequest(MfaRequest request)
    {
        if (request == null)
            throw new Hephaestus.Application.Exceptions.ValidationException("Dados de MFA s�o obrigat�rios.", new ValidationResult());

        if (string.IsNullOrEmpty(request.Email))
            throw new Hephaestus.Application.Exceptions.ValidationException("E-mail � obrigat�rio.", new ValidationResult());

        if (string.IsNullOrEmpty(request.MfaCode))
            throw new Hephaestus.Application.Exceptions.ValidationException("C�digo MFA � obrigat�rio.", new ValidationResult());
    }

    /// <summary>
    /// Valida o e-mail fornecido.
    /// </summary>
    /// <param name="email">E-mail a ser validado.</param>
    private void ValidateEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            throw new Hephaestus.Application.Exceptions.ValidationException("E-mail � obrigat�rio.", new ValidationResult());
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
    /// Valida o c�digo MFA.
    /// </summary>
    /// <param name="email">E-mail da empresa.</param>
    /// <param name="mfaCode">C�digo MFA a ser validado.</param>
    private async Task ValidateMfaCodeAsync(string email, string mfaCode)
    {
        var isValid = await _mfaService.ValidateMfaCodeAsync(email, mfaCode);
        ValidateBusinessRule(isValid, "C�digo MFA inv�lido.", "MFA_VALIDATION");
    }

    /// <summary>
    /// Cria o log de auditoria.
    /// </summary>
    /// <param name="company">Empresa.</param>
    /// <param name="action">A��o realizada.</param>
    /// <param name="details">Detalhes da a��o.</param>
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
