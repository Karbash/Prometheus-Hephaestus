using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Hephaestus.Infrastructure.Services;

/// <summary>
/// Servi�o para recuperar informa��es do usu�rio logado.
/// </summary>
public class LoggedUserService : ILoggedUserService
{
    private readonly ICompanyRepository _companyRepository;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="LoggedUserService"/>.
    /// </summary>
    /// <param name="companyRepository">Reposit�rio de empresas.</param>
    public LoggedUserService(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    /// <summary>
    /// Obt�m as informa��es do usu�rio logado com base no token JWT.
    /// </summary>
    /// <param name="claimsPrincipal">Claims do usu�rio autenticado.</param>
    /// <returns>Dados do usu�rio logado (ID, nome, e-mail, fun��o).</returns>
    /// <exception cref="InvalidOperationException">E-mail n�o encontrado no token ou usu�rio n�o existe.</exception>
    public async Task<LoggedUser> GetLoggedUserAsync(ClaimsPrincipal claimsPrincipal)
    {
        var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            throw new InvalidOperationException("E-mail n�o encontrado no token.");

        var company = await _companyRepository.GetByEmailAsync(email);
        if (company == null)
            throw new InvalidOperationException("Usu�rio n�o encontrado.");

        return new LoggedUser(
            Id: company.Id,
            Name: company.Name,
            Email: company.Email,
            Role: company.Role.ToString()
        );
    }

    /// <summary>
    /// Obt�m o ID do usu�rio logado.
    /// </summary>
    /// <param name="claimsPrincipal">Claims do usu�rio autenticado.</param>
    /// <returns>ID do usu�rio.</returns>
    /// <exception cref="InvalidOperationException">ID n�o encontrado no token.</exception>
    public string GetUserId(ClaimsPrincipal claimsPrincipal)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claimsPrincipal.FindFirst("sub")?.Value
            ?? claimsPrincipal.FindFirst("UserId")?.Value;

        if (string.IsNullOrEmpty(userId))
            throw new InvalidOperationException("ID do usu�rio n�o encontrado no token.");

        return userId;
    }

    /// <summary>
    /// Obt�m o ID do tenant do usu�rio logado.
    /// </summary>
    /// <param name="claimsPrincipal">Claims do usu�rio autenticado.</param>
    /// <returns>ID do tenant.</returns>
    /// <exception cref="InvalidOperationException">TenantId n�o encontrado no token.</exception>
    public string GetTenantId(ClaimsPrincipal claimsPrincipal)
    {
        var tenantId = claimsPrincipal.FindFirst("TenantId")?.Value;

        if (string.IsNullOrEmpty(tenantId))
            throw new InvalidOperationException("TenantId n�o encontrado no token.");

        return tenantId;
    }

    public string GetCompanyId(System.Security.Claims.ClaimsPrincipal claimsPrincipal)
    {
        var companyId = claimsPrincipal.FindFirst("CompanyId")?.Value;
        if (string.IsNullOrEmpty(companyId))
        {
            // fallback para TenantId para compatibilidade
            companyId = claimsPrincipal.FindFirst("TenantId")?.Value;
        }
        if (string.IsNullOrEmpty(companyId))
            throw new InvalidOperationException("CompanyId não encontrado no token.");
        return companyId;
    }
}
