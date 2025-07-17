using Hephaestus.Domain.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Menu;

/// <summary>
/// Interface para o caso de uso de listagem de itens do card�pio.
/// </summary>
public interface IGetMenuItemsUseCase
{
    /// <summary>
    /// Lista itens do card�pio do tenant.
    /// </summary>
    /// <param name="user">Usu�rio autenticado.</param>
    /// <param name="pageNumber">N�mero da p�gina.</param>
    /// <param name="pageSize">Tamanho da p�gina.</param>
    /// <returns>Lista de itens do card�pio.</returns>
    Task<PagedResult<MenuItemResponse>> ExecuteAsync(System.Security.Claims.ClaimsPrincipal user, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc", List<string>? tagIds = null, List<string>? categoryIds = null, decimal? maxPrice = null, bool? promotionActiveNow = null, int? promotionDayOfWeek = null, string? promotionTime = null);
}
