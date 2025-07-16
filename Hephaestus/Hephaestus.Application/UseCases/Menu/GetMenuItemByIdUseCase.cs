using Hephaestus.Domain.DTOs.Response;
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
/// Caso de uso para obten��o de item do card�pio por ID.
/// </summary>
public class GetMenuItemByIdUseCase : BaseUseCase, IGetMenuItemByIdUseCase
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="GetMenuItemByIdUseCase"/>.
    /// </summary>
    /// <param name="menuItemRepository">Reposit�rio de itens do card�pio.</param>
    /// <param name="loggedUserService">Servi�o para obter informa��es do usu�rio logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
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
    /// Executa a obten��o de um item do card�pio por ID.
    /// </summary>
    /// <param name="id">ID do item do card�pio.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    /// <returns>Item do card�pio encontrado.</returns>
    public async Task<MenuItemResponse> ExecuteAsync(string id, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            
            // Valida��o dos par�metros de entrada
            ValidateInputParameters(id, tenantId);

            // Busca o item do card�pio
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
                // AvailableAdditionalIds foi removido da entidade
                ImageUrl = menuItem.ImageUrl
            };
        });
    }

    /// <summary>
    /// Valida os par�metros de entrada.
    /// </summary>
    /// <param name="id">ID do item do card�pio.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private void ValidateInputParameters(string id, string tenantId)
    {
        if (string.IsNullOrEmpty(id))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do item do card�pio � obrigat�rio.", new ValidationResult());

        if (string.IsNullOrEmpty(tenantId))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do tenant � obrigat�rio.", new ValidationResult());
    }
}
