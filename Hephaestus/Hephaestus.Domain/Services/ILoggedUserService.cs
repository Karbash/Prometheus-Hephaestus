using System.Security.Claims;

namespace Hephaestus.Domain.Services;

/// <summary>
/// Interface para recuperar informa��es do usu�rio logado.
/// </summary>
public interface ILoggedUserService
{
    /// <summary>
    /// Obt�m as informa��es do usu�rio logado com base no token JWT.
    /// </summary>
    /// <param name="claimsPrincipal">Claims do usu�rio autenticado.</param>
    /// <returns>Dados do usu�rio logado (ID, nome, e-mail, fun��o).</returns>
    /// <exception cref="InvalidOperationException">Usu�rio n�o autenticado ou dados inv�lidos.</exception>
    Task<LoggedUser> GetLoggedUserAsync(ClaimsPrincipal claimsPrincipal);

    /// <summary>
    /// Obt�m o ID do usu�rio logado.
    /// </summary>
    /// <param name="claimsPrincipal">Claims do usu�rio autenticado.</param>
    /// <returns>ID do usu�rio.</returns>
    /// <exception cref="InvalidOperationException">ID n�o encontrado no token.</exception>
    string GetUserId(ClaimsPrincipal claimsPrincipal);

    /// <summary>
    /// Obt�m o ID do tenant do usu�rio logado.
    /// </summary>
    /// <param name="claimsPrincipal">Claims do usu�rio autenticado.</param>
    /// <returns>ID do tenant.</returns>
    /// <exception cref="InvalidOperationException">TenantId n�o encontrado no token.</exception>
    string GetTenantId(ClaimsPrincipal claimsPrincipal);

    /// <summary>
    /// Obt�m o ID da empresa do usurio logado.
    /// </summary>
    /// <param name="claimsPrincipal">Claims do usu�rio autenticado.</param>
    /// <returns>ID da empresa.</returns>
    /// <exception cref="InvalidOperationException">CompanyId no encontrado no token.</exception>
    string GetCompanyId(System.Security.Claims.ClaimsPrincipal claimsPrincipal);
}

/// <summary>
/// Representa os dados do usurio logado.
/// </summary>
public record LoggedUser(string Id, string Name, string Email, string Role);
