using Hephaestus.Domain.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Menu;

/// <summary>
/// Interface para o caso de uso de obten��o de um item do card�pio por ID.
/// </summary>
public interface IGetMenuItemByIdUseCase
{
    /// <summary>
    /// Obt�m detalhes de um item do card�pio por ID.
    /// </summary>
    /// <param name="id">ID do item.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    /// <returns>Detalhes do item do card�pio.</returns>
    Task<MenuItemResponse> ExecuteAsync(string id, ClaimsPrincipal user);
}
