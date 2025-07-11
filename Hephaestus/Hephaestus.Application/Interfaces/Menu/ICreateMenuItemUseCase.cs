using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Interfaces.Menu;

/// <summary>
/// Interface para o caso de uso de criação de itens do cardápio.
/// </summary>
public interface ICreateMenuItemUseCase
{
    /// <summary>
    /// Cria um novo item do cardápio para o tenant.
    /// </summary>
    /// <param name="request">Dados do item do cardápio.</param>
    /// <param name="tenantId">ID do tenant extraído do token JWT.</param>
    /// <returns>ID do item criado.</returns>
    Task<string> ExecuteAsync(CreateMenuItemRequest request, string tenantId);
}