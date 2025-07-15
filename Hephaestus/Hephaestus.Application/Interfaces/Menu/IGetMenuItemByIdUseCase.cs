using Hephaestus.Domain.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Menu;

/// <summary>
/// Interface para o caso de uso de obtenção de um item do cardápio por ID.
/// </summary>
public interface IGetMenuItemByIdUseCase
{
    /// <summary>
    /// Obtém detalhes de um item do cardápio por ID.
    /// </summary>
    /// <param name="id">ID do item.</param>
    /// <param name="user">Usuário autenticado.</param>
    /// <returns>Detalhes do item do cardápio.</returns>
    Task<MenuItemResponse> ExecuteAsync(string id, ClaimsPrincipal user);
}