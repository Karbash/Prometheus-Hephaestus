namespace Hephaestus.Application.Interfaces.Menu;

/// <summary>
/// Interface para o caso de uso de exclusão de itens do cardápio.
/// </summary>
public interface IDeleteMenuItemUseCase
{
    /// <summary>
    /// Remove um item do cardápio.
    /// </summary>
    /// <param name="id">ID do item.</param>
    /// <param name="tenantId">ID do tenant extraído do token JWT.</param>
    Task ExecuteAsync(string id, string tenantId);
}