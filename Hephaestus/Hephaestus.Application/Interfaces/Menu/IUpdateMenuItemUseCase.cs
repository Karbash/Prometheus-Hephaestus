using Hephaestus.Application.DTOs.Request;

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
    /// <param name="tenantId">ID do tenant extraído do token JWT.</param>
    Task ExecuteAsync(string id, UpdateMenuItemRequest request, string tenantId);
}