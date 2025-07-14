using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Menu;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Services;
using FluentValidation.Results;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Menu;

/// <summary>
/// Caso de uso para obtenção de item do cardápio por ID.
/// </summary>
public class GetMenuItemByIdUseCase : BaseUseCase, IGetMenuItemByIdUseCase
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="GetMenuItemByIdUseCase"/>.
    /// </summary>
    /// <param name="menuItemRepository">Repositório de itens do cardápio.</param>
    /// <param name="loggedUserService">Serviço para obter informações do usuário logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public GetMenuItemByIdUseCase(
        IMenuItemRepository menuItemRepository,
        ILoggedUserService loggedUserService,
        ILogger<GetMenuItemByIdUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _menuItemRepository = menuItemRepository;
        _loggedUserService = loggedUserService;
    }

    /// <summary>
    /// Executa a obtenção de um item do cardápio por ID.
    /// </summary>
    /// <param name="id">ID do item do cardápio.</param>
    /// <param name="user">Usuário autenticado.</param>
    /// <returns>Item do cardápio encontrado.</returns>
    public async Task<MenuItemResponse> ExecuteAsync(string id, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            
            // Validação dos parâmetros de entrada
            ValidateInputParameters(id, tenantId);

            // Busca o item do cardápio
            var menuItem = await _menuItemRepository.GetByIdAsync(id, tenantId);
            
            // Verifica se o item existe
            EnsureEntityExists(menuItem, "MenuItem", id);

            // Mapeia para a resposta
            return new MenuItemResponse
            {
                Id = menuItem.Id,
                TenantId = menuItem.TenantId,
                Name = menuItem.Name,
                Description = menuItem.Description,
                CategoryId = menuItem.CategoryId,
                Price = menuItem.Price,
                IsAvailable = menuItem.IsAvailable,
                TagIds = menuItem.MenuItemTags?.Select(mt => mt.TagId).ToList() ?? new List<string>(),
                AvailableAdditionalIds = menuItem.AvailableAdditionalIds,
                ImageUrl = menuItem.ImageUrl
            };
        });
    }

    /// <summary>
    /// Valida os parâmetros de entrada.
    /// </summary>
    /// <param name="id">ID do item do cardápio.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private void ValidateInputParameters(string id, string tenantId)
    {
        if (string.IsNullOrEmpty(id))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do item do cardápio é obrigatório.", new ValidationResult());

        if (string.IsNullOrEmpty(tenantId))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do tenant é obrigatório.", new ValidationResult());
    }
}