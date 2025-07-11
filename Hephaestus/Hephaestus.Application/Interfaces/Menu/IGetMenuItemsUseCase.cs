using Hephaestus.Application.DTOs.Response;

namespace Hephaestus.Application.Interfaces.Menu;

/// <summary>
/// Interface para o caso de uso de listagem de itens do cardápio.
/// </summary>
public interface IGetMenuItemsUseCase
{
    /// <summary>
    /// Lista itens do cardápio do tenant.
    /// </summary>
    /// <param name="tenantId">ID do tenant extraído do token JWT.</param>
    /// <returns>Lista de itens do cardápio.</returns>
    Task<IEnumerable<MenuItemResponse>> ExecuteAsync(string tenantId);
}