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
/// Caso de uso refatorado para autenticação de usuários com tratamento robusto de exceções.
/// </summary>
public class LoginUseCaseRefactored : BaseUseCase, ILoginUseCase
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IConfiguration _configuration;
    private readonly IMfaService _mfaService;

    public LoginUseCaseRefactored(
        ICompanyRepository companyRepository,
        IConfiguration configuration,
        IMfaService mfaService,
        ILogger<LoginUseCaseRefactored> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _companyRepository = companyRepository;
        _configuration = configuration;
        _mfaService = mfaService;
    }

    /// <summary>
    /// Autentica um usuário e retorna um token JWT com tratamento robusto de exceções.
    /// </summary>
    /// <param name="request">Dados de login (e-mail e senha).</param>
    /// <param name="mfaCode">Código MFA opcional para administradores.</param>
    /// <returns>Token JWT gerado.</returns>
    public async Task<string> ExecuteAsync(LoginRequest request, string? mfaCode = null)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação de entrada
            ValidateLoginRequest(request);

            // Autenticação do usuário
            var company = await AuthenticateUserAsync(request);

            // Validação MFA para administradores
            await ValidateMfaForAdminAsync(company, mfaCode);

            // Geração do token
            var token = GenerateJwtToken(company);

            Logger.LogInformation("Login realizado com sucesso para o usuário {Email}", company.Email);

            return token;
        }, "Login");
    }

    /// <summary>
    /// Valida os dados de entrada do login.
    /// </summary>
    private void ValidateLoginRequest(LoginRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request), "Dados de login são obrigatórios.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new BusinessRuleException("E-mail é obrigatório.", "EmailRequired");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new BusinessRuleException("Senha é obrigatória.", "PasswordRequired");
    }

    /// <summary>
    /// Autentica o usuário com as credenciais fornecidas.
    /// </summary>
    private async Task<Domain.Entities.Company> AuthenticateUserAsync(LoginRequest request)
    {
        var company = await _companyRepository.GetByEmailAsync(request.Email);
        
        if (company == null)
        {
            Logger.LogWarning("Tentativa de login com e-mail inexistente: {Email}", request.Email);
            throw new BusinessRuleException("Credenciais inválidas.", "InvalidCredentials");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, company.PasswordHash))
        {
            Logger.LogWarning("Tentativa de login com senha incorreta para: {Email}", request.Email);
            throw new BusinessRuleException("Credenciais inválidas.", "InvalidCredentials");
        }

        if (!company.IsEnabled)
        {
            Logger.LogWarning("Tentativa de login em conta desabilitada: {Email}", request.Email);
            throw new BusinessRuleException("Conta desabilitada. Entre em contato com o administrador.", "AccountDisabled");
        }

        return company;
    }

    /// <summary>
    /// Valida o código MFA para administradores.
    /// </summary>
    private async Task ValidateMfaForAdminAsync(Domain.Entities.Company company, string? mfaCode)
    {
        if (company.Role == Role.Admin && !string.IsNullOrEmpty(company.MfaSecret))
        {
            if (string.IsNullOrEmpty(mfaCode))
            {
                Logger.LogWarning("MFA necessário para administrador: {Email}", company.Email);
                throw new BusinessRuleException("Código MFA necessário para administradores.", "MfaRequired");
            }

            var isValid = await _mfaService.ValidateMfaCodeAsync(company.Email, mfaCode);
            if (!isValid)
            {
                Logger.LogWarning("Código MFA inválido para administrador: {Email}", company.Email);
                throw new BusinessRuleException("Código MFA inválido.", "InvalidMfaCode");
            }
        }
    }

    /// <summary>
    /// Gera o token JWT para o usuário autenticado.
    /// </summary>
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
            Expires = DateTime.UtcNow.AddHours(24),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
} 
