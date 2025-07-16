using Hephaestus.Domain.DTOs.Request;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Menu;

/// <summary>
/// Interface para o caso de uso de atualiza��o de itens do card�pio.
/// </summary>
public interface IUpdateMenuItemUseCase
{
    /// <summary>
    /// Atualiza um item do card�pio.
    /// </summary>
    /// <param name="id">ID do item.</param>
    /// <param name="request">Dados atualizados do item.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    Task ExecuteAsync(string id, UpdateMenuItemRequest request, ClaimsPrincipal user);
}
