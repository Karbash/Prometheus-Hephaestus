using Hephaestus.Application.DTOs.Request;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Menu;

/// <summary>
/// Interface para o caso de uso de atualização de itens do cardápio.
/// </summary>
public interface IUpdateMenuItemUseCase
{
    /// <summary>
    /// Atualiza um item do cardápio.
    /// </summary>
    /// <param name="id">ID do item.</param>
    /// <param name="request">Dados atualizados do item.</param>
    /// <param name="user">Usuário autenticado.</param>
    Task ExecuteAsync(string id, UpdateMenuItemRequest request, ClaimsPrincipal user);
}