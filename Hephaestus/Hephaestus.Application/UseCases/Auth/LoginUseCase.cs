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

namespace Hephaestus.Application.UseCases.Auth;

/// <summary>
/// Caso de uso para autenticação de usuários.
/// </summary>
public class LoginUseCase : ILoginUseCase
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
    public LoginUseCase(ICompanyRepository companyRepository, IConfiguration configuration, IMfaService mfaService)
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
    /// <exception cref="InvalidOperationException">Credenciais inválidas ou código MFA inválido.</exception>
    public async Task<string> ExecuteAsync(LoginRequest request, string? mfaCode = null)
    {
        var company = await _companyRepository.GetByEmailAsync(request.Email);
        if (company == null || !BCrypt.Net.BCrypt.Verify(request.Password, company.PasswordHash))
            throw new InvalidOperationException("Credenciais inválidas.");

        if (company.Role == Role.Admin && !string.IsNullOrEmpty(company.MfaSecret))
        {
            if (string.IsNullOrEmpty(mfaCode))
                throw new InvalidOperationException("Código MFA necessário para administradores.");
            var isValid = await _mfaService.ValidateMfaCodeAsync(request.Email, mfaCode);
            if (!isValid)
                throw new InvalidOperationException("Código MFA inválido.");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, company.Id),
            new Claim(ClaimTypes.Name, company.Name ?? "Unknown"),
            new Claim(ClaimTypes.Email, company.Email),
            new Claim(ClaimTypes.Role, company.Role.ToString()),
            new Claim("TenantId", company.Role == Role.Tenant ? company.Id : string.Empty), // Define TenantId para Role=Tenant
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