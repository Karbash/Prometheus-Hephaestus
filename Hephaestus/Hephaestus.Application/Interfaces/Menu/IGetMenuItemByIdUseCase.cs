using Hephaestus.Application.DTOs.Response;

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
    /// <param name="tenantId">ID do tenant extraído do token JWT.</param>
    /// <returns>Detalhes do item do cardápio.</returns>
    Task<MenuItemResponse> ExecuteAsync(string id, string tenantId);
}