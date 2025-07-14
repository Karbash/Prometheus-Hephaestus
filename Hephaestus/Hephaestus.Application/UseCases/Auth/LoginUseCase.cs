using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Auth;
using Hephaestus.Domain.Enum;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;

namespace Hephaestus.Application.UseCases.Auth;

/// <summary>
/// Caso de uso para autenticação de usuários.
/// </summary>
public class LoginUseCase : BaseUseCase, ILoginUseCase
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IConfiguration _configuration;
    private readonly IMfaService _mfaService;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="LoginUseCase"/>.
    /// </summary>
    /// <param name="companyRepository">Repositório de empresas.</param>
    /// <param name="configuration">Configuração da aplicação.</param>
    /// <param name="mfaService">Serviço de autenticação multifator.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
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
    /// Autentica um usuário e retorna um token JWT.
    /// </summary>
    /// <param name="request">Dados de login (e-mail e senha).</param>
    /// <param name="mfaCode">Código MFA opcional para administradores.</param>
    /// <returns>Token JWT gerado.</returns>
    public async Task<string> ExecuteAsync(LoginRequest request, string? mfaCode = null)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação dos dados de entrada
            ValidateLoginRequest(request);

            // Autenticação do usuário
            var company = await AuthenticateUserAsync(request);

            // Validação MFA para administradores
            await ValidateMfaForAdminAsync(company, mfaCode);

            // Geração do token JWT
            return GenerateJwtToken(company);
        });
    }

    /// <summary>
    /// Valida os dados da requisição de login.
    /// </summary>
    /// <param name="request">Requisição de login.</param>
    private void ValidateLoginRequest(LoginRequest request)
    {
        if (request == null)
            throw new ValidationException("Dados de login são obrigatórios.", new ValidationResult());

        if (string.IsNullOrEmpty(request.Email))
            throw new ValidationException("E-mail é obrigatório.", new ValidationResult());

        if (string.IsNullOrEmpty(request.Password))
            throw new ValidationException("Senha é obrigatória.", new ValidationResult());
    }

    /// <summary>
    /// Autentica o usuário com as credenciais fornecidas.
    /// </summary>
    /// <param name="request">Dados de login.</param>
    /// <returns>Empresa autenticada.</returns>
    private async Task<Domain.Entities.Company> AuthenticateUserAsync(LoginRequest request)
    {
        var company = await _companyRepository.GetByEmailAsync(request.Email);
        
        if (company == null || !BCrypt.Net.BCrypt.Verify(request.Password, company.PasswordHash))
        {
            throw new UnauthorizedException("Credenciais inválidas.", "LOGIN", "Authentication");
        }

        return company;
    }

    /// <summary>
    /// Valida o código MFA para administradores.
    /// </summary>
    /// <param name="company">Empresa autenticada.</param>
    /// <param name="mfaCode">Código MFA.</param>
    private async Task ValidateMfaForAdminAsync(Domain.Entities.Company company, string? mfaCode)
    {
        if (company.Role == Role.Admin && !string.IsNullOrEmpty(company.MfaSecret))
        {
            if (string.IsNullOrEmpty(mfaCode))
            {
                throw new UnauthorizedException("Código MFA necessário para administradores.", "MFA_VALIDATION", "Authentication");
            }

            var isValid = await _mfaService.ValidateMfaCodeAsync(company.Email, mfaCode);
            if (!isValid)
            {
                throw new UnauthorizedException("Código MFA inválido.", "MFA_VALIDATION", "Authentication");
            }
        }
    }

    /// <summary>
    /// Gera o token JWT para o usuário autenticado.
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