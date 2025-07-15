using Hephaestus.Domain.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Menu;

/// <summary>
/// Interface para o caso de uso de listagem de itens do cardápio.
/// </summary>
public interface IGetMenuItemsUseCase
{
    /// <summary>
    /// Lista itens do cardápio do tenant.
    /// </summary>
    /// <param name="user">Usuário autenticado.</param>
    /// <param name="pageNumber">Número da página.</param>
    /// <param name="pageSize">Tamanho da página.</param>
    /// <returns>Lista de itens do cardápio.</returns>
    Task<PagedResult<MenuItemResponse>> ExecuteAsync(System.Security.Claims.ClaimsPrincipal user, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
}