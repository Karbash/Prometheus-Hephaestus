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
    public async Task<PagedResult<MenuItemResponse>> ExecuteAsync(ClaimsPrincipal user, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            ValidateInputParameters(tenantId);
            var pagedMenuItems = await _menuItemRepository.GetByTenantIdAsync(tenantId, pageNumber, pageSize, sortBy, sortOrder);
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
    /// <param name="tenantId">ID do tenant.</param>
    private void ValidateInputParameters(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do tenant � obrigat�rio.", new ValidationResult());
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
            TenantId = m.TenantId,
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
