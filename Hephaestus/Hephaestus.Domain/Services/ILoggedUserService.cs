using System.Security.Claims;

namespace Hephaestus.Domain.Services;

/// <summary>
/// Interface para recuperar informações do usuário logado.
/// </summary>
public interface ILoggedUserService
{
    /// <summary>
    /// Obtém as informações do usuário logado com base no token JWT.
    /// </summary>
    /// <param name="claimsPrincipal">Claims do usuário autenticado.</param>
    /// <returns>Dados do usuário logado (ID, nome, e-mail, função).</returns>
    /// <exception cref="InvalidOperationException">Usuário não autenticado ou dados inválidos.</exception>
    Task<LoggedUser> GetLoggedUserAsync(ClaimsPrincipal claimsPrincipal);

    /// <summary>
    /// Obtém o ID do usuário logado.
    /// </summary>
    /// <param name="claimsPrincipal">Claims do usuário autenticado.</param>
    /// <returns>ID do usuário.</returns>
    /// <exception cref="InvalidOperationException">ID não encontrado no token.</exception>
    string GetUserId(ClaimsPrincipal claimsPrincipal);
}

/// <summary>
/// Representa os dados do usuário logado.
/// </summary>
public record LoggedUser(string Id, string Name, string Email, string Role);