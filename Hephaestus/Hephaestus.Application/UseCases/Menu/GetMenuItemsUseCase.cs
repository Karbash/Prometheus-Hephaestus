using FluentValidation.Results;
using Hephaestus.Application.Base;
using Hephaestus.Application.Interfaces.Menu;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Hephaestus.Domain.DTOs.Response;

namespace Hephaestus.Application.UseCases.Menu;

/// <summary>
/// Caso de uso para obter todos os itens do card�pio de um tenant.
/// </summary>
public class GetMenuItemsUseCase : BaseUseCase, IGetMenuItemsUseCase
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="GetMenuItemsUseCase"/>.
    /// </summary>
    /// <param name="menuItemRepository">Reposit�rio de itens do card�pio.</param>
    /// <param name="loggedUserService">Servi�o para obter informa��es do usu�rio logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
    public GetMenuItemsUseCase(
        IMenuItemRepository menuItemRepository,
        ILoggedUserService loggedUserService,
        ILogger<GetMenuItemsUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _menuItemRepository = menuItemRepository;
        _loggedUserService = loggedUserService;
    }

    /// <summary>
    /// Executa a busca de todos os itens do card�pio de um tenant.
    /// </summary>
    /// <param name="user">Usu�rio autenticado.</param>
    /// <param name="pageNumber">N�mero da p�gina (padr�o: 1).</param>
    /// <param name="pageSize">Tamanho da p�gina (padr�o: 20).</param>
    /// <returns>Lista paginada de itens do card�pio.</returns>
    public async Task<PagedResult<MenuItemResponse>> ExecuteAsync(ClaimsPrincipal user, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc", List<string>? tagIds = null, List<string>? categoryIds = null, decimal? maxPrice = null, bool? promotionActiveNow = null, int? promotionDayOfWeek = null, string? promotionTime = null)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var companyId = _loggedUserService.GetCompanyId(user);
            ValidateInputParameters(companyId);
            var pagedMenuItems = await _menuItemRepository.GetByCompanyIdAsync(companyId, pageNumber, pageSize, sortBy, sortOrder, tagIds, categoryIds, maxPrice, promotionActiveNow, promotionDayOfWeek, promotionTime);
            return new PagedResult<MenuItemResponse>
            {
                Items = ConvertToResponseDtos(pagedMenuItems.Items).ToList(),
                TotalCount = pagedMenuItems.TotalCount,
                PageNumber = pagedMenuItems.PageNumber,
                PageSize = pagedMenuItems.PageSize
            };
        });
    }

    /// <summary>
    /// Valida os par�metros de entrada.
    /// </summary>
    /// <param name="companyId">ID da empresa.</param>
    private void ValidateInputParameters(string companyId)
    {
        if (string.IsNullOrEmpty(companyId))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID da empresa é obrigatório.", new ValidationResult());
    }

    /// <summary>
    /// Converte as entidades para DTOs de resposta.
    /// </summary>
    /// <param name="menuItems">Lista de itens do card�pio.</param>
    /// <returns>Lista de DTOs de resposta.</returns>
    private IEnumerable<MenuItemResponse> ConvertToResponseDtos(IEnumerable<Domain.Entities.MenuItem> menuItems)
    {
        return menuItems.Select(m => new MenuItemResponse
        {
            Id = m.Id,
            CompanyId = m.CompanyId,
            Name = m.Name,
            Description = m.Description,
            CategoryId = m.CategoryId,
            Price = m.Price,
            IsAvailable = m.IsAvailable,
            TagIds = m.MenuItemTags.Select(mt => mt.TagId).ToList(),
            // AvailableAdditionalIds foi removido da entidade
            ImageUrl = m.ImageUrl
        }).ToList();
    }
}
