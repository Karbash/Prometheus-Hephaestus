using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Menu;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using FluentValidation.Results;

namespace Hephaestus.Application.UseCases.Menu;

/// <summary>
/// Caso de uso para obter todos os itens do cardápio de um tenant.
/// </summary>
public class GetMenuItemsUseCase : BaseUseCase, IGetMenuItemsUseCase
{
    private readonly IMenuItemRepository _menuItemRepository;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="GetMenuItemsUseCase"/>.
    /// </summary>
    /// <param name="menuItemRepository">Repositório de itens do cardápio.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public GetMenuItemsUseCase(
        IMenuItemRepository menuItemRepository,
        ILogger<GetMenuItemsUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _menuItemRepository = menuItemRepository;
    }

    /// <summary>
    /// Executa a busca de todos os itens do cardápio de um tenant.
    /// </summary>
    /// <param name="tenantId">ID do tenant.</param>
    /// <returns>Lista de itens do cardápio.</returns>
    public async Task<IEnumerable<MenuItemResponse>> ExecuteAsync(string tenantId)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação dos parâmetros de entrada
            ValidateInputParameters(tenantId);

            // Busca dos itens do cardápio
            var menuItems = await GetMenuItemsAsync(tenantId);

            // Conversão para DTOs de resposta
            return ConvertToResponseDtos(menuItems);
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
    /// Busca os itens do cardápio.
    /// </summary>
    /// <param name="tenantId">ID do tenant.</param>
    /// <returns>Lista de itens do cardápio.</returns>
    private async Task<IEnumerable<Domain.Entities.MenuItem>> GetMenuItemsAsync(string tenantId)
    {
        return await _menuItemRepository.GetByTenantIdAsync(tenantId);
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
            ImageUrl = m.ImageUrl
        }).ToList();
    }
}