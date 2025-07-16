using Hephaestus.Application.Base;
using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Interfaces.Auth;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Enum;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Hephaestus.Application.UseCases.Auth;

/// <summary>
/// Caso de uso para autentica��o de usu�rios.
/// </summary>
public class LoginUseCase : BaseUseCase, ILoginUseCase
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IConfiguration _configuration;
    private readonly IMfaService _mfaService;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="LoginUseCase"/>.
    /// </summary>
    /// <param name="companyRepository">Reposit�rio de empresas.</param>
    /// <param name="configuration">Configura��o da aplica��o.</param>
    /// <param name="mfaService">Servi�o de autentica��o multifator.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
    public LoginUseCase(
        ICompanyRepository companyRepository, 
        IConfiguration configuration, 
        IMfaService mfaService,
        ILogger<LoginUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _companyRepository = companyRepository;
        _configuration = configuration;
        _mfaService = mfaService;
    }

    /// <summary>
    /// Autentica um usu�rio e retorna um token JWT.
    /// </summary>
    /// <param name="request">Dados de login (e-mail e senha).</param>
    /// <param name="mfaCode">C�digo MFA opcional para administradores.</param>
    /// <returns>Token JWT gerado.</returns>
    public async Task<string> ExecuteAsync(LoginRequest request, string? mfaCode = null)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Valida��o dos dados de entrada
            // Remover todas as linhas que usam _validator

            // Autentica��o do usu�rio
            var company = await AuthenticateUserAsync(request);

            // Valida��o MFA para administradores
            await ValidateMfaForAdminAsync(company, mfaCode);

            // Gera��o do token JWT
            return GenerateJwtToken(company);
        });
    }

    /// <summary>
    /// Autentica o usu�rio com as credenciais fornecidas.
    /// </summary>
    /// <param name="request">Dados de login.</param>
    /// <returns>Empresa autenticada.</returns>
    private async Task<Domain.Entities.Company> AuthenticateUserAsync(LoginRequest request)
    {
        var company = await _companyRepository.GetByEmailAsync(request.Email);
        
        if (company == null || !BCrypt.Net.BCrypt.Verify(request.Password, company.PasswordHash))
        {
            throw new UnauthorizedException("Credenciais inv�lidas.", "LOGIN", "Authentication");
        }

        return company;
    }

    /// <summary>
    /// Valida o c�digo MFA para administradores.
    /// </summary>
    /// <param name="company">Empresa autenticada.</param>
    /// <param name="mfaCode">C�digo MFA.</param>
    private async Task ValidateMfaForAdminAsync(Domain.Entities.Company company, string? mfaCode)
    {
        if (company.Role == Role.Admin && !string.IsNullOrEmpty(company.MfaSecret))
        {
            if (string.IsNullOrEmpty(mfaCode))
            {
                throw new UnauthorizedException("C�digo MFA necess�rio para administradores.", "MFA_VALIDATION", "Authentication");
            }

            var isValid = await _mfaService.ValidateMfaCodeAsync(company.Email, mfaCode);
            if (!isValid)
            {
                throw new UnauthorizedException("C�digo MFA inv�lido.", "MFA_VALIDATION", "Authentication");
            }
        }
    }

    /// <summary>
    /// Gera o token JWT para o usu�rio autenticado.
    /// </summary>
    /// <param name="company">Empresa autenticada.</param>
    /// <returns>Token JWT.</returns>
    private string GenerateJwtToken(Domain.Entities.Company company)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, company.Id),
            new Claim(ClaimTypes.Name, company.Name ?? "Unknown"),
            new Claim(ClaimTypes.Email, company.Email),
            new Claim(ClaimTypes.Role, company.Role.ToString()),
            new Claim("TenantId", company.Role == Role.Tenant ? company.Id : string.Empty),
            new Claim("MfaValidated", company.Role == Role.Admin && !string.IsNullOrEmpty(company.MfaSecret) ? "true" : "false")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
