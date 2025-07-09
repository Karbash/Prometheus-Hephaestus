using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Hephaestus.Application.UseCases;

public class LoginUseCase : ILoginUseCase
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IConfiguration _configuration;

    public LoginUseCase(ICompanyRepository companyRepository, IConfiguration configuration)
    {
        _companyRepository = companyRepository;
        _configuration = configuration;
    }

    public async Task<string> ExecuteAsync(LoginRequest request)
    {
        var company = await _companyRepository.GetByEmailAsync(request.Email);
        if (company == null || !BCrypt.Net.BCrypt.Verify(request.Password, company.PasswordHash))
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        if (!company.IsEnabled)
            throw new UnauthorizedAccessException("Empresa desativada.");

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
                new Claim("TenantId", company.Role == Role.Tenant ? company.Id : string.Empty)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}