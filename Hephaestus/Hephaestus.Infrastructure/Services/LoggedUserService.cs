using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Hephaestus.Infrastructure.Services;

/// <summary>
/// Serviço para recuperar informações do usuário logado.
/// </summary>
public class LoggedUserService : ILoggedUserService
{
    private readonly ICompanyRepository _companyRepository;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="LoggedUserService"/>.
    /// </summary>
    /// <param name="companyRepository">Repositório de empresas.</param>
    public LoggedUserService(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    /// <summary>
    /// Obtém as informações do usuário logado com base no token JWT.
    /// </summary>
    /// <param name="claimsPrincipal">Claims do usuário autenticado.</param>
    /// <returns>Dados do usuário logado (ID, nome, e-mail, função).</returns>
    /// <exception cref="InvalidOperationException">E-mail não encontrado no token ou usuário não existe.</exception>
    public async Task<LoggedUser> GetLoggedUserAsync(ClaimsPrincipal claimsPrincipal)
    {
        var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            throw new InvalidOperationException("E-mail não encontrado no token.");

        var company = await _companyRepository.GetByEmailAsync(email);
        if (company == null)
            throw new InvalidOperationException("Usuário não encontrado.");

        return new LoggedUser(
            Id: company.Id,
            Name: company.Name,
            Email: company.Email,
            Role: company.Role.ToString()
        );
    }

    /// <summary>
    /// Obtém o ID do usuário logado.
    /// </summary>
    /// <param name="claimsPrincipal">Claims do usuário autenticado.</param>
    /// <returns>ID do usuário.</returns>
    /// <exception cref="InvalidOperationException">ID não encontrado no token.</exception>
    public string GetUserId(ClaimsPrincipal claimsPrincipal)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claimsPrincipal.FindFirst("sub")?.Value
            ?? claimsPrincipal.FindFirst("UserId")?.Value;

        if (string.IsNullOrEmpty(userId))
            throw new InvalidOperationException("ID do usuário não encontrado no token.");

        return userId;
    }

    /// <summary>
    /// Obtém o ID do tenant do usuário logado.
    /// </summary>
    /// <param name="claimsPrincipal">Claims do usuário autenticado.</param>
    /// <returns>ID do tenant.</returns>
    /// <exception cref="InvalidOperationException">TenantId não encontrado no token.</exception>
    public string GetTenantId(ClaimsPrincipal claimsPrincipal)
    {
        var tenantId = claimsPrincipal.FindFirst("TenantId")?.Value;

        if (string.IsNullOrEmpty(tenantId))
            throw new InvalidOperationException("TenantId não encontrado no token.");

        return tenantId;
    }
}
