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
/// Caso de uso para obter todos os itens do cardápio de um tenant.
/// </summary>
public class GetMenuItemsUseCase : BaseUseCase, IGetMenuItemsUseCase
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="GetMenuItemsUseCase"/>.
    /// </summary>
    /// <param name="menuItemRepository">Repositório de itens do cardápio.</param>
    /// <param name="loggedUserService">Serviço para obter informações do usuário logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
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
    /// Executa a busca de todos os itens do cardápio de um tenant.
    /// </summary>
    /// <param name="user">Usuário autenticado.</param>
    /// <param name="pageNumber">Número da página (padrão: 1).</param>
    /// <param name="pageSize">Tamanho da página (padrão: 20).</param>
    /// <returns>Lista paginada de itens do cardápio.</returns>
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
    /// Valida os parâmetros de entrada.
    /// </summary>
    /// <param name="tenantId">ID do tenant.</param>
    private void ValidateInputParameters(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do tenant é obrigatório.", new ValidationResult());
    }

    /// <summary>
    /// Converte as entidades para DTOs de resposta.
    /// </summary>
    /// <param name="menuItems">Lista de itens do cardápio.</param>
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
            AvailableAdditionalIds = m.AvailableAdditionalIds,
            ImageUrl = m.ImageUrl,
            Tags = m.MenuItemTags.Select(mt => mt.Tag.Name).ToList()
        }).ToList();
    }
}