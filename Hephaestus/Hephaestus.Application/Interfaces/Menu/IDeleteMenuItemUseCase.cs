using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Menu;

/// <summary>
/// Interface para o caso de uso de exclus�o de itens do card�pio.
/// </summary>
public interface IDeleteMenuItemUseCase
{
    /// <summary>
    /// Remove um item do card�pio.
    /// </summary>
    /// <param name="id">ID do item.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    Task ExecuteAsync(string id, ClaimsPrincipal user);
}
