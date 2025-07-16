using Hephaestus.Domain.DTOs.Request;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Menu;

/// <summary>
/// Interface para o caso de uso de cria��o de itens do card�pio.
/// </summary>
public interface ICreateMenuItemUseCase
{
    /// <summary>
    /// Cria um novo item do card�pio para o tenant.
    /// </summary>
    /// <param name="request">Dados do item do card�pio.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    /// <returns>ID do item criado.</returns>
    Task<string> ExecuteAsync(CreateMenuItemRequest request, ClaimsPrincipal user);
}
