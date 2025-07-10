using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Auth;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Enum;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Hephaestus.Application.UseCases.Auth;

/// <summary>
/// Caso de uso para autenticação multifator (MFA).
/// </summary>
public class MfaUseCase : IMfaUseCase
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
    public MfaUseCase(
        ICompanyRepository companyRepository,
        IConfiguration configuration,
        IMfaService mfaService,
        IAuditLogRepository auditLogRepository)
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
    /// <exception cref="InvalidOperationException">Código MFA inválido ou usuário não é administrador.</exception>
    public async Task<string> ExecuteAsync(MfaRequest request)
    {
        var company = await _companyRepository.GetByEmailAsync(request.Email);
        if (company == null || company.Role != Role.Admin)
            throw new InvalidOperationException("Apenas administradores podem usar MFA.");

        var isValid = await _mfaService.ValidateMfaCodeAsync(request.Email, request.MfaCode);
        if (!isValid)
            throw new InvalidOperationException("Código MFA inválido.");

        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = company.Id,
            Action = "Validação MFA",
            EntityId = company.Id,
            Details = $"MFA validado para {company.Email}.",
            CreatedAt = DateTime.UtcNow
        });

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
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
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Configura o MFA para um administrador, gerando um segredo TOTP.
    /// </summary>
    /// <param name="email">E-mail do administrador.</param>
    /// <returns>Segredo TOTP para configuração no aplicativo autenticador.</returns>
    /// <exception cref="InvalidOperationException">Apenas administradores podem configurar MFA.</exception>
    public async Task<string> SetupMfaAsync(string email)
    {
        var company = await _companyRepository.GetByEmailAsync(email);
        if (company == null || company.Role != Role.Admin)
            throw new InvalidOperationException("Apenas administradores podem configurar MFA.");

        var secret = await _mfaService.GenerateMfaSecretAsync(email);

        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = company.Id,
            Action = "Configuração MFA",
            EntityId = company.Id,
            Details = $"MFA configurado para {company.Email}.",
            CreatedAt = DateTime.UtcNow
        });

        return secret;
    }
}